#if !BESTHTTP_DISABLE_SIGNALR_CORE

using System;
using UnityEngine;
using BestHTTP.SignalRCore;
using BestHTTP.SignalRCore.Encoders;
using UnityEngine.UI;
using BestHTTP.Examples.Helpers;

namespace BestHTTP.Examples
{
    // Server side of this example can be found here:
    // https://github.com/Benedicht/BestHTTP_DemoSite/blob/master/BestHTTP_DemoSite/Hubs/TestHub.cs
    public class TestHubSample : BestHTTP.Examples.Helpers.SampleBase
    {
#pragma warning disable 0649
        [SerializeField]
        private string _path = "/TestHub";

        [SerializeField]
        private ScrollRect _scrollRect;

        [SerializeField]
        private RectTransform _contentRoot;

        [SerializeField]
        private TextListItem _listItemPrefab;

        [SerializeField]
        private int _maxListItemEntries = 100;

        [SerializeField]
        private Button _connectButton;

        [SerializeField]
        private Button _closeButton;

#pragma warning restore
        
        // Instance of the HubConnection
        HubConnection hub;

        protected override void Start()
        {
            base.Start();

            SetButtons(true, false);
        }

        void OnDestroy()
        {
            if (hub != null)
                hub.StartClose();
        }

        /// <summary>
        /// GUI button callback
        /// </summary>
        public void OnConnectButton()
        {
            // Set up optional options
            HubOptions options = new HubOptions();
            options.SkipNegotiation = false;

            // Crete the HubConnection
            hub = new HubConnection(new Uri(base.sampleSelector.BaseURL + this._path), new JsonProtocol(new LitJsonEncoder()), options);

            // Optionally add an authenticator
            //hub.AuthenticationProvider = new BestHTTP.SignalRCore.Authentication.HeaderAuthenticator("<generated jwt token goes here>");

            // Subscribe to hub events
            hub.OnConnected += Hub_OnConnected;
            hub.OnError += Hub_OnError;
            hub.OnClosed += Hub_OnClosed;
            
            // Set up server callable functions
            hub.On("Send", (string arg) => AddText(string.Format("On '<color=green>Send</color>': '<color=yellow>{0}</color>'", arg)).AddLeftPadding(20));
            hub.On<Person>("Person", (person) => AddText(string.Format("On '<color=green>Person</color>': '<color=yellow>{0}</color>'", person)).AddLeftPadding(20));
            hub.On<Person, Person>("TwoPersons", (person1, person2) => AddText(string.Format("On '<color=green>TwoPersons</color>': '<color=yellow>{0}</color>', '<color=yellow>{1}</color>'", person1, person2)).AddLeftPadding(20));

            // And finally start to connect to the server
            hub.StartConnect();

            AddText("StartConnect called");

            SetButtons(false, false);
        }

        /// <summary>
        /// GUI button callback
        /// </summary>
        public void OnCloseButton()
        {
            if (this.hub != null)
            {
                this.hub.StartClose();

                AddText("StartClose called");
                SetButtons(false, false);
            }
        }

        /// <summary>
        /// This callback is called when the plugin is connected to the server successfully. Messages can be sent to the server after this point.
        /// </summary>
        private void Hub_OnConnected(HubConnection hub)
        {
            SetButtons(false, true);
            AddText("Hub Connected");

            // Call a server function with a string param. We expect no return value.
            hub.Send("Send", "my message");

            // Call a parameterless function. We expect a string return value.
            hub.Invoke<string>("NoParam")
                .OnSuccess(ret => AddText(string.Format("'<color=green>NoParam' returned: '<color=yellow>{0}</color>'", ret)).AddLeftPadding(20));

            // Call a function on the server to add two numbers. OnSuccess will be called with the result and OnError if there's an error.
            hub.Invoke<int>("Add", 10, 20)
                .OnSuccess(result => AddText(string.Format("'<color=green>Add(10, 20)</color>' returned: '<color=yellow>{0}</color>'", result)).AddLeftPadding(20))
                .OnError(error => AddText(string.Format("'<color=green>Add(10, 20)</color>' error: '<color=red>{0}</color>'", error)).AddLeftPadding(20));

            hub.Invoke<int?>("NullableTest", 10)
                .OnSuccess(result => AddText(string.Format("'<color=green>NullableTest(10)</color>' returned: '<color=yellow>{0}</color>'", result)).AddLeftPadding(20))
                .OnError(error => AddText(string.Format("'<color=green>NullableTest(10)</color>' error: '<color=red>{0}</color>'", error)).AddLeftPadding(20));

            // Call a function that will return a Person object constructed from the function's parameters.
            hub.Invoke<Person>("GetPerson", "Mr. Smith", 26)
                .OnSuccess(result => AddText(string.Format("'<color=green>GetPerson(\"Mr. Smith\", 26)</color>' returned: '<color=yellow>{0}</color>'", result)).AddLeftPadding(20))
                .OnError(error => AddText(string.Format("'<color=green>GetPerson(\"Mr. Smith\", 26)</color>' error: '<color=red>{0}</color>'", error)).AddLeftPadding(20));

            // To test errors/exceptions this call always throws an exception on the server side resulting in an OnError call.
            // OnError expected here!
            hub.Invoke<int>("SingleResultFailure", 10, 20)
                .OnSuccess(result => AddText(string.Format("'<color=green>SingleResultFailure(10, 20)</color>' returned: '<color=yellow>{0}</color>'", result)).AddLeftPadding(20))
                .OnError(error => AddText(string.Format("'<color=green>SingleResultFailure(10, 20)</color>' error: '<color=red>{0}</color>'", error)).AddLeftPadding(20));

            // This call demonstrates IEnumerable<> functions, result will be the yielded numbers.
            hub.Invoke<int[]>("Batched", 10)
                .OnSuccess(result => AddText(string.Format("'<color=green>Batched(10)</color>' returned items: '<color=yellow>{0}</color>'", result.Length)).AddLeftPadding(20))
                .OnError(error => AddText(string.Format("'<color=green>Batched(10)</color>' error: '<color=red>{0}</color>'", error)).AddLeftPadding(20));

            // OnItem is called for a streaming request for every items returned by the server. OnSuccess will still be called with all the items.
            hub.GetDownStreamController<int>("ObservableCounter", 10, 1000)
                .OnItem(result => AddText(string.Format("'<color=green>ObservableCounter(10, 1000)</color>' OnItem: '<color=yellow>{0}</color>'", result)).AddLeftPadding(20))
                .OnSuccess(result => AddText("'<color=green>ObservableCounter(10, 1000)</color>' OnSuccess.").AddLeftPadding(20))
                .OnError(error => AddText(string.Format("'<color=green>ObservableCounter(10, 1000)</color>' error: '<color=red>{0}</color>'", error)).AddLeftPadding(20));

            // A stream request can be cancelled any time.
            var controller = hub.GetDownStreamController<int>("ChannelCounter", 10, 1000);

            controller.OnItem(result => AddText(string.Format("'<color=green>ChannelCounter(10, 1000)</color>' OnItem: '<color=yellow>{0}</color>'", result)).AddLeftPadding(20))
                      .OnSuccess(result => AddText("'<color=green>ChannelCounter(10, 1000)</color>' OnSuccess.").AddLeftPadding(20))
                      .OnError(error => AddText(string.Format("'<color=green>ChannelCounter(10, 1000)</color>' error: '<color=red>{0}</color>'", error)).AddLeftPadding(20));

            // a stream can be cancelled by calling the controller's Cancel method
            controller.Cancel();

            // This call will stream strongly typed objects
            hub.GetDownStreamController<Person>("GetRandomPersons", 20, 2000)
                .OnItem(result => AddText(string.Format("'<color=green>GetRandomPersons(20, 1000)</color>' OnItem: '<color=yellow>{0}</color>'", result)).AddLeftPadding(20))
                .OnSuccess(result => AddText("'<color=green>GetRandomPersons(20, 1000)</color>' OnSuccess.").AddLeftPadding(20));
        }

        /// <summary>
        /// This is called when the hub is closed after a StartClose() call.
        /// </summary>
        private void Hub_OnClosed(HubConnection hub)
        {
            SetButtons(true, false);
            AddText("Hub Closed");
        }

        /// <summary>
        /// Called when an unrecoverable error happen. After this event the hub will not send or receive any messages.
        /// </summary>
        private void Hub_OnError(HubConnection hub, string error)
        {
            SetButtons(true, false);
            AddText(string.Format("Hub Error: <color=red>{0}</color>", error));
        }

        private void SetButtons(bool connect, bool close)
        {
            if (this._connectButton != null)
                this._connectButton.interactable = connect;

            if (this._closeButton != null)
                this._closeButton.interactable = close;
        }

        private TextListItem AddText(string text)
        {
            return GUIHelper.AddText(this._listItemPrefab, this._contentRoot, text, this._maxListItemEntries, this._scrollRect);
        }

        /// <summary>
        /// Helper class to demonstrate strongly typed callbacks
        /// </summary>
        sealed class Person
        {
            public string Name { get; set; }
            public long Age { get; set; }

            public override string ToString()
            {
                return string.Format("[Person Name: '{0}', Age: '<color=yellow>{1}</color>']", this.Name, this.Age.ToString());
            }
        }
    }
}

#endif