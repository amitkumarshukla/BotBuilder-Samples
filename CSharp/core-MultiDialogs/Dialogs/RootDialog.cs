namespace MultiDialogsBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using MultiDialogsBot.Luis;

    [Serializable]
    public class MyAppCard
    {
        public string AppName { get; set; }
        public string AppHandler { get; set; }
    }

    

    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private const string FlightsOption = "Flights";

        private const string HotelsOption = "Hotels";

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;
            var reply = context.MakeMessage();
            reply.Attachments = new List<Attachment>();

            Rootobject outputObj = await OpenAppLuis.MakeRequest(message.Text);

            string appname = "";
            string apphandler = "";

            if (outputObj.entities != null && outputObj.entities.Length != 0)
            {
                appname = outputObj.entities[0].entity;
            }

            if (outputObj.topScoringIntent != null && outputObj.topScoringIntent.intent != null)
            {
                apphandler = outputObj.topScoringIntent.intent;
            }

            var MyAppCardobj = new MyAppCard()
            {
                AppName = appname,
                AppHandler = apphandler
            };

            var outAttachment = new Attachment()
            {
                ContentType = "application/json",
                Content = MyAppCardobj
            };
            reply.Attachments.Add(outAttachment);

            await context.PostAsync(reply);
            context.Wait(this.MessageReceivedAsync);
        }
    }
}