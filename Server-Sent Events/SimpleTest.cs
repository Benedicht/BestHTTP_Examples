#if !BESTHTTP_DISABLE_SERVERSENT_EVENTS

using System;
using BestHTTP.ServerSentEvents;
using UnityEngine;

namespace BestHTTP.Examples
{
    public class SimpleTest : MonoBehaviour
    {
        string address = GUIHelper.BaseURL + "/sse";

        EventSource eventSource;

        /// <summary>
        /// Debug text to draw on the gui
        /// </summary>
        string Text = string.Empty;

        /// <summary>
        /// GUI scroll position
        /// </summary>
        Vector2 scrollPos;

        void OnGUI()
        {
            GUIHelper.DrawArea(GUIHelper.ClientArea, true, () =>
            {
                scrollPos = GUILayout.BeginScrollView(scrollPos);
                GUILayout.Label(Text);
                GUILayout.EndScrollView();

                GUILayout.Space(5);

                GUILayout.FlexibleSpace();

                address = GUILayout.TextField(address);

                if (this.eventSource == null && GUILayout.Button("Open Server-Sent Events"))
                {
                    // Create the EventSource instance
                    this.eventSource = new EventSource(new Uri(this.address));

                    // Subscribe to generic events
                    this.eventSource.OnOpen += OnOpen;
                    this.eventSource.OnClosed += OnClosed;
                    this.eventSource.OnError += OnError;
                    this.eventSource.OnStateChanged += this.OnStateChanged;
                    this.eventSource.OnMessage += OnMessage;

                    // Subscribe to an application specific event
                    this.eventSource.On("datetime", OnDateTime);

                    // Start to connect to the server
                    this.eventSource.Open();

                    Text += "Opening Server-Sent Events...\n";
                }

                if (this.eventSource != null && this.eventSource.State == States.Open)
                {
                    GUILayout.Space(10);

                    if (GUILayout.Button("Close"))
                    {
                        // Close the connection
                        this.eventSource.Close();
                    }
                }
            });
        }

        private void OnOpen(EventSource eventSource)
        {
            this.Text += "Open\n";
        }

        private void OnClosed(EventSource eventSource)
        {
            this.Text += "Closed\n";
            this.eventSource = null;
        }

        private void OnError(EventSource eventSource, string error)
        {
            this.Text += "Error: " + error + "\n";
        }

        private void OnStateChanged(EventSource eventSource, States oldState, States newState)
        {
            this.Text += string.Format("State Changed {0} => {1}\n", oldState, newState);
        }

        private void OnMessage(EventSource eventSource, Message message)
        {
            this.Text += string.Format("Message: {0}\n", message);
        }

        private void OnDateTime(EventSource eventSource, Message message)
        {
            DateTimeData dtData = LitJson.JsonMapper.ToObject<DateTimeData>(message.Data);
            this.Text += string.Format("OnDateTime: {0}\n", dtData.ToString());
        }

        void OnDestroy()
        {
            if (this.eventSource != null)
            {
                this.eventSource.Close();
                this.eventSource = null;
            }
        }
    }

    sealed class DateTimeData
    {
#pragma warning disable 0649
        public int eventid;
        public string datetime;
#pragma warning restore

        public override string ToString()
        {
            return string.Format("[DateTimeData EventId: {0}, DateTime: {1}]", this.eventid, this.datetime);
        }
    }
}
#endif