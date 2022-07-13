using AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EcommerceAdminBot.Utilities;
using EcommerceAdminBot;

namespace ToDoBot.Dialogs.Operations
{
    public class DeleteTaskDialog : ComponentDialog
    {
        private readonly CosmoDBClientToDo _cosmosDBClient;
        public DeleteTaskDialog(CosmoDBClientToDo cosmosDBClient) : base(nameof(DeleteTaskDialog))
        {
            _cosmosDBClient = cosmosDBClient;

            var waterfallSteps = new WaterfallStep[]
            {
                ShowTasksStepAsync,
                DeleteTasksStepAsync,
                DeleteMoreTasksStepAsync,
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> ShowTasksStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            List<ToDoTask> toDoTasks = await _cosmosDBClient.QueryItemsAsync(User.UserID);

            if (toDoTasks.Count == 0)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("You don't have any tasks to delete."), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

            List<string> taskList = new List<string>();
            for (int i = 0; i < toDoTasks.Count; i++)
            {
                taskList.Add(toDoTasks[i].Task);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Please select the tasks you want to delete."), cancellationToken);

            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                // Use LINQ to turn the choices into submit actions
                Actions = taskList.Select(choice => new AdaptiveSubmitAction
                {
                    Title = choice,
                    Data = choice,  // This will be a string
                }).ToList<AdaptiveAction>(),
            };
            // Prompt
            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = (Activity)MessageFactory.Attachment(new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    // Convert the AdaptiveCard to a JObject
                    Content = JObject.FromObject(card),
                }),
                Choices = ChoiceFactory.ToChoices(taskList),
                // Don't render the choices outside the card
                Style = ListStyle.None,
            },
                cancellationToken);
        }

        private async Task<DialogTurnResult> DeleteTasksStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["TaskToDelete"] = ((FoundChoice)stepContext.Result).Value;
            string taskToDelete = (string)stepContext.Values["TaskToDelete"];
            bool deleteTask = await _cosmosDBClient.DeleteTaskItemAsync(taskToDelete, User.UserID);

            if (deleteTask)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Task '" + taskToDelete + "' successfully deleted."), cancellationToken);

                List<ToDoTask> toDoTasks = await _cosmosDBClient.QueryItemsAsync(User.UserID);

                if (toDoTasks.Count == 0)
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("No Tasks left. All your tasks are deleted."), cancellationToken);
                    return await stepContext.EndDialogAsync(null, cancellationToken);
                }

                return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions
                {
                    Prompt = MessageFactory.Text("Would you like to Delete more tasks?")
                }, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Task '" + taskToDelete + "' could not be deleted. Either it has been already deleted or some error occurred."), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }

        }

        private async Task<DialogTurnResult> DeleteMoreTasksStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                return await stepContext.ReplaceDialogAsync(InitialDialogId, null, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Ok."));
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
        }
    }
}
