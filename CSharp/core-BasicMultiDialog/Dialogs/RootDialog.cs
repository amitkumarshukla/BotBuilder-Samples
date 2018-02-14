namespace BasicMultiDialogBot.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    #pragma warning disable 1998

    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private const string currentState = "CurrentState";

        private string name;
        private string age;

        public async Task StartAsync(IDialogContext context)
        {
            /* Wait until the first message is received from the conversation and call MessageReceviedAsync 
             *  to process that message. */
            context.Wait(this.MessageReceivedAsync);
        }

        private int GetCurrentState(IDialogContext context)
        {
            int curstate = 0;

            foreach (Microsoft.Bot.Connector.Entity entity in context.Activity.Entities)
            {
                if (entity.Type == "CurrentState")
                {
                    BotCurrentDialogState statenumber = entity.GetAs<BotCurrentDialogState>();
                    curstate = statenumber.CurState;
                    break;
                }
            }
            return curstate;
        }

        private Microsoft.Bot.Connector.Entity SetNextStateEntity(int nextState)
        {
            Microsoft.Bot.Connector.Entity entity = new Microsoft.Bot.Connector.Entity(currentState);

            BotCurrentDialogState s = new BotCurrentDialogState(nextState);
            entity.SetAs<BotCurrentDialogState>(s);
            entity.Type = currentState;

            return entity;

        }
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            /* When MessageReceivedAsync is called, it's passed an IAwaitable<IMessageActivity>. To get the message,
             *  await the result. */
            var message = await result;

            int state = GetCurrentState(context);

            switch(state)
            {
                case 0:
                    await this.SendWelcomeMessageAsync(context);
                    await NameFunction(context);
                    break;

                case 1:
                    await AgeFunction(context);
                    break;

                case 2:
                    await FinishConversation(context);
                    break;

                default:
                    break;

            }

            context.Wait(this.MessageReceivedAsync);
        }

        public async Task FinishConversation(IDialogContext context)
        {
            age = context.Activity.AsMessageActivity().Text;
            var reply = ConstructReply(context, $"Your name is { name } and your age is { age }.", 0);
            await context.PostAsync(reply);
        }

        public async Task AgeFunction(IDialogContext context)
        {
            name = context.Activity.AsMessageActivity().Text;

            var reply = ConstructReply(context, $"{ name }, what is your age?", 2);

            await context.PostAsync(reply);
        }

        public async Task NameFunction(IDialogContext context)
        {
            var reply = ConstructReply(context,"What is your name",1);
            await context.PostAsync(reply);
        }

        private async Task SendWelcomeMessageAsync(IDialogContext context)
        {
            await context.PostAsync("Hi, I'm the Basic Multi Dialog bot. Let's get started.");
        }

        private IMessageActivity ConstructReply(IDialogContext context, string text, int nextState)
        {
            var reply = context.MakeMessage();

            var entity = SetNextStateEntity(nextState);

            if (reply.Entities == null)
            {
                reply.Entities = new List<Microsoft.Bot.Connector.Entity>();
            }

            reply.Entities.Add(entity);
            reply.Text = text;

            return reply;

        }


    }
}