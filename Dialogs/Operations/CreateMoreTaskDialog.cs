using EcommerceAdminBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System.Threading;
using System.Threading.Tasks;

namespace EcommerceAdminBot.Dialogs.Operations
{
    public class CreateMoreTaskDialog : ComponentDialog
    {
        public CreateMoreTaskDialog() : base(nameof(CreateMoreTaskDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                TasksStepAsync,
                ConfirmStepAsync,
                SummaryStepAsync,
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> TasksStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("Please give the task to add.")
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userDetails = (User)stepContext.Options;
            stepContext.Values["Task"] = (string)stepContext.Result;
            userDetails.TasksList.Add((string)stepContext.Values["Task"]);

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("Would you like to Add more tasks?")
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var userDetails = (User)stepContext.Options;
            if ((bool)stepContext.Result)
            {
                return await stepContext.ReplaceDialogAsync(InitialDialogId, userDetails, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Ok."));
                return await stepContext.EndDialogAsync(userDetails, cancellationToken);
            }
        }
    }
}
