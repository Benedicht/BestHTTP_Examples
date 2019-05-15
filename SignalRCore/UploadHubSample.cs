#if !BESTHTTP_DISABLE_SIGNALR_CORE

using BestHTTP;
using BestHTTP.SignalRCore;
using BestHTTP.SignalRCore.Encoders;
using System;
using System.Collections;
using UnityEngine;

namespace BestHTTP.Examples
{
    sealed class Person
    {
        public string Name { get; set; }
        public long Age { get; set; }

        public override string ToString()
        {
            return string.Format("[Person Name: '{0}', Age: {1}]", this.Name, this.Age.ToString());
        }
    }

    /// <summary>
    /// This sample demonstrates redirection capabilities. The server will redirect a few times the client before
    /// routing it to the final endpoint.
    /// </summary>
    public sealed class UploadHubSample : MonoBehaviour
    {
        // Server uri to connect to
        readonly Uri URI = new Uri(GUIHelper.BaseURL + "/uploading");

        // Instance of the HubConnection
        public HubConnection hub;

        Vector2 scrollPos;
        public string uiText;

        private const float YieldWaitTime = 0.1f;

        void Start()
        {
            HubOptions options = new HubOptions();
            options.SkipNegotiation = true;

            // Crete the HubConnection
            hub = new HubConnection(URI, new JsonProtocol(new LitJsonEncoder()), options);

            // Subscribe to hub events
            hub.OnConnected += Hub_OnConnected;
            hub.OnError += Hub_OnError;
            hub.OnClosed += Hub_OnClosed;

            hub.OnMessage += Hub_OnMessage;

            hub.OnRedirected += Hub_Redirected;

            // And finally start to connect to the server
            hub.StartConnect();

            uiText = "StartConnect called\n";
        }

        void OnDestroy()
        {
            if (hub != null)
            {
                hub.StartClose();
            }
        }

        // Draw the text stored in the 'uiText' field
        void OnGUI()
        {
            GUIHelper.DrawArea(GUIHelper.ClientArea, true, () =>
            {
                scrollPos = GUILayout.BeginScrollView(scrollPos, false, false);
                GUILayout.BeginVertical();

                GUILayout.Label(uiText);

                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            });
        }

        private void Hub_Redirected(HubConnection hub, Uri oldUri, Uri newUri)
        {
            uiText += string.Format("Hub connection redirected to '<color=green>{0}</color>'!\n", hub.Uri);
        }

        /// <summary>
        /// This callback is called when the plugin is connected to the server successfully. Messages can be sent to the server after this point.
        /// </summary>
        private void Hub_OnConnected(HubConnection hub)
        {
            uiText += "Hub Connected\n";

            StartCoroutine(UploadWord());
        }

        private IEnumerator UploadWord()
        {
            uiText += "\n<color=green>UploadWord</color>:\n";
            var controller = hub.Upload<string, string>("UploadWord");
            controller.OnComplete(result =>
                {
                    uiText += string.Format("-UploadWord completed, result: '<color=yellow>{0}</color>'\n", result.value);

                    StartCoroutine(ScoreTracker());
                });

            yield return new WaitForSeconds(YieldWaitTime);
            controller.Upload("Hello ");
            uiText += "-'<color=green>Hello </color>' uploaded!\n";

            yield return new WaitForSeconds(YieldWaitTime);
            controller.Upload("World");
            uiText += "-'<color=green>World</color>' uploaded!\n";

            yield return new WaitForSeconds(YieldWaitTime);
            controller.Upload("!!");
            uiText += "-'<color=green>!!</color>' uploaded!\n";

            yield return new WaitForSeconds(YieldWaitTime);
            controller.Finish();
            uiText += "-Sent upload finished message.\n";
            yield return new WaitForSeconds(YieldWaitTime);
        }

        private IEnumerator ScoreTracker()
        {
            uiText += "\n<color=green>ScoreTracker</color>:\n";
            var controller = hub.UploadStream<string, int, int>("ScoreTracker");
            controller.OnComplete(result =>
                {
                    uiText += string.Format("-ScoreTracker completed, result: '<color=yellow>{0}</color>'\n", result.value);

                    StartCoroutine(ScoreTrackerWithParameterChannels());
                });

            const int numScores = 5;
            for (int i = 0; i < numScores; i++)
            {
                yield return new WaitForSeconds(YieldWaitTime);

                int p1 = UnityEngine.Random.Range(0, 10);
                int p2 = UnityEngine.Random.Range(0, 10);
                controller.Upload(p1, p2);

                uiText += string.Format("-Score({0}/{1}) uploaded! p1's score: <color=green>{2}</color> p2's score: <color=green>{3}</color>\n", i + 1, numScores, p1, p2);
            }

            yield return new WaitForSeconds(YieldWaitTime);
            controller.Finish();
            uiText += "-Sent upload finished message.\n";
            yield return new WaitForSeconds(YieldWaitTime);
        }

        private IEnumerator ScoreTrackerWithParameterChannels()
        {
            uiText += "\n<color=green>ScoreTracker using upload channels</color>:\n";
            using (var controller = hub.UploadStream<string, int, int>("ScoreTracker"))
            {
                controller.OnComplete(result =>
                {
                    uiText += string.Format("-ScoreTracker completed, result: '<color=yellow>{0}</color>'\n", result.value);

                    StartCoroutine(StreamEcho());
                });

                const int numScores = 5;

                // While the server's ScoreTracker has two parameters, we can upload those parameters separately
                // So here we 

                using (var player1param = controller.GetUploadChannel<int>(0))
                {
                    for (int i = 0; i < numScores; i++)
                    {
                        yield return new WaitForSeconds(YieldWaitTime);

                        int score = UnityEngine.Random.Range(0, 10);
                        player1param.Upload(score);

                        uiText += string.Format("-Player 1's score({0}/{1}) uploaded! Score: <color=green>{2}</color>\n", i + 1, numScores, score);
                    }
                }

                uiText += "\n";

                using (var player2param = controller.GetUploadChannel<int>(1))
                {
                    for (int i = 0; i < numScores; i++)
                    {
                        yield return new WaitForSeconds(YieldWaitTime);

                        int score = UnityEngine.Random.Range(0, 10);
                        player2param.Upload(score);

                        uiText += string.Format("-Player 2's score({0}/{1}) uploaded! Score: <color=green>{2}</color>\n", i + 1, numScores, score);
                    }
                }

                uiText += "\n-All scores uploaded!\n";
            }
            yield return new WaitForSeconds(YieldWaitTime);
        }

        private IEnumerator StreamEcho()
        {
            uiText += "\n<color=green>StreamEcho</color>:\n";
            using (var controller = hub.UploadStreamWithDownStream<string, string>("StreamEcho"))
            {
                controller.OnComplete(result =>
                {
                    uiText += "-StreamEcho completed!\n";

                    StartCoroutine(PersonEcho());
                });

                controller.OnItem(item =>
                {
                    uiText += string.Format("-Received from server: '<color=yellow>{0}</color>'\n", item.LastAdded);
                });

                const int numMessages = 5;
                for (int i = 0; i < numMessages; i++)
                {
                    yield return new WaitForSeconds(YieldWaitTime);

                    string message = string.Format("Message from client {0}/{1}", i + 1, numMessages);
                    controller.Upload(message);

                    uiText += string.Format("-Sent message to the server: <color=green>{0}</color>\n", message);
                }

                yield return new WaitForSeconds(YieldWaitTime);
            }
            uiText += "-Upload finished!\n";
            yield return new WaitForSeconds(YieldWaitTime);
        }

        /// <summary>
        /// This is basically the same as the previous StreamEcho, but it's streaming a complex object (Person
        /// </summary>
        private IEnumerator PersonEcho()
        {
            uiText += "\n<color=green>PersonEcho</color>:\n";

            using (var controller = hub.UploadStreamWithDownStream<Person, Person>("PersonEcho"))
            {
                controller.OnComplete(result =>
                {
                    uiText += "-PersonEcho completed!\n";
                });

                controller.OnItem(item =>
                {
                    uiText += string.Format("-Received from server: '<color=yellow>{0}</color>'\n", item.LastAdded);
                });

                const int numMessages = 5;
                for (int i = 0; i < numMessages; i++)
                {
                    yield return new WaitForSeconds(YieldWaitTime);

                    Person person = new Person()
                    {
                        Name = "Mr. Smith",
                        Age = 20 + i * 2
                    };

                    controller.Upload(person);

                    uiText += string.Format("-Sent person to the server: <color=green>{0}</color>\n", person);
                }

                yield return new WaitForSeconds(YieldWaitTime);
            }
            uiText += "-Upload finished!\n";

            yield return new WaitForSeconds(YieldWaitTime);
        }

        /// <summary>
        /// This callback is called for every hub message. If false is returned, the plugin will cancel any further processing of the message.
        /// </summary>
        private bool Hub_OnMessage(HubConnection hub, BestHTTP.SignalRCore.Messages.Message message)
        {
            //uiText += string.Format("( Message received: {0} )\n", message.ToString());

            return true;
        }

        /// <summary>
        /// This is called when the hub is closed after a StartClose() call.
        /// </summary>
        private void Hub_OnClosed(HubConnection hub)
        {
            uiText += "Hub Closed\n";
        }

        /// <summary>
        /// Called when an unrecoverable error happen. After this event the hub will not send or receive any messages.
        /// </summary>
        private void Hub_OnError(HubConnection hub, string error)
        {
            uiText += "Hub Error: " + error + "\n";
        }
    }
}

#endif