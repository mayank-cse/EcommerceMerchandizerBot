using EcommerceAdminBot.Models;
using EcommerceAdminBot.Services;
using EcommerceAdminBot.Utilities;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EcommerceAdminBot.Dialogs.Operations
{
    public class EmailAuthenticationDialog : ComponentDialog
    {
        private readonly StateService _stateService;
        private readonly UserRepository _userRespository;
        protected readonly IConfiguration Configuration;
        private readonly string EmailDialogID = "EmailDlg";
        private readonly string EmailVerificationCodeDialogID = "EmailVerificationCodeDlg";
        public EmailAuthenticationDialog(StateService stateService, UserRepository userRepository, IConfiguration configuration) : base(nameof(EmailAuthenticationDialog))
        {
            _stateService = stateService ?? throw new System.ArgumentNullException(nameof(stateService));
            _userRespository = userRepository;
            Configuration = configuration;

            var waterfallSteps = new WaterfallStep[]
            {
                EmailStepAsync,
                OTPVerificationStepAsync,
                AuthenticationConfirmStepAsync,
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new TextPrompt(EmailDialogID, EmailValidation));
            AddDialog(new NumberPrompt<int>(EmailVerificationCodeDialogID, EmailVerificationCodeValidation));

            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> EmailStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Please Authenticate..."), cancellationToken);
            return await stepContext.PromptAsync(EmailDialogID, new PromptOptions
            {
                Prompt = MessageFactory.Text("Can I have your email address please?"),

            }, cancellationToken);
        }

        private async Task<DialogTurnResult> OTPVerificationStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());

            // Save the email to user profile
            userProfile.Email = (string)stepContext.Result;

            // Generate the OTP and save it to the user profile
            Random rnd = new Random();
            userProfile.OTP = rnd.Next(100000, 999999);

            await _stateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Please wait while I send an OTP to your email{userProfile.Email} for verification."), cancellationToken);

            // trigger the power automate flow to send email
            bool status = await _userRespository.SendEmailForCodeVerificationAsync(userProfile.OTP, userProfile.Email, userProfile.Name, Configuration["PowerAutomatePOSTURL"]);

            // verify the response status of the api call
            if (status)
            {
                return await stepContext.PromptAsync(EmailVerificationCodeDialogID, new PromptOptions
                {
                    Prompt = MessageFactory.Text("I have sent a verification code to your email. Please enter the code here to validate your email. Please Check your inbox or Spam folder for the email.")
                }, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Verfication Email could not be sent. Please try again later. Sorry for the inconvenience caused."), cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }


        }

        private async Task<DialogTurnResult> AuthenticationConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Your email is verified."), cancellationToken);
            UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());
            userProfile.UserAuthenticated = true;
            await _stateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private async Task<bool> EmailValidation(PromptValidatorContext<string> promptcontext, CancellationToken cancellationtoken)
        {
            string email = promptcontext.Recognized.Value;

            if (string.IsNullOrWhiteSpace(email))
            {
                await promptcontext.Context.SendActivityAsync("The email you entered is not valid, please enter a valid email.", cancellationToken: cancellationtoken);
                return false;
            }

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                if (addr.Address == email)
                {
                    return true;
                }
                else
                {
                    await promptcontext.Context.SendActivityAsync("The email you entered is not valid, please enter a valid email.", cancellationToken: cancellationtoken);
                    return false;
                }
            }
            catch
            {
                await promptcontext.Context.SendActivityAsync("The email you entered is not valid, please enter a valid email.", cancellationToken: cancellationtoken);
                return false;
            }
        }

        private async Task<bool> EmailVerificationCodeValidation(PromptValidatorContext<int> promptcontext, CancellationToken cancellationtoken)
        {
            UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(promptcontext.Context, () => new UserProfile());
            int verificationCode = promptcontext.Recognized.Value;

            if (verificationCode == userProfile.OTP)
            {
                return true;
            }
            await promptcontext.Context.SendActivityAsync("The verification code you entered is incorrect. Please enter the correct code.", cancellationToken: cancellationtoken);
            return false;
        }
    }
}