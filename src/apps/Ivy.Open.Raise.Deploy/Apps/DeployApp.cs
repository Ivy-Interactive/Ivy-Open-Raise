using Ivy.Open.Raise.Deploy.Apps.Deploy;

namespace Ivy.Open.Raise.Deploy.Apps;

[App(icon: Icons.Rocket, title: "Deploy Open-Raise")]
public class DeployApp : ViewBase
{
    public override object? Build()
    {
        // Single model state
        var deployment = UseState(() => new DeploymentModel());

        // Form validation state
        var formErrors = UseState<Dictionary<string, string>>(() => []);
        var isSubmitting = UseState(false);

        var client = UseService<IClientProvider>();

        // Handle form submission
        void HandleSubmit()
        {
            if (isSubmitting.Value) return;

            var errors = DeploymentValidator.ValidateForm(deployment.Value);
            if (errors.Count > 0)
            {
                formErrors.Set(errors);
                client.Toast("Please fix the validation errors before submitting.");
                return;
            }

            isSubmitting.Set(true);

            try
            {
                // Simulate form submission
                client.Toast($"Deployment created for {deployment.Value.ProjectName}!");
            }
            finally
            {
                isSubmitting.Set(false);
            }
        }

        return Layout.Vertical().Padding(4)
            | new Card(
                new DeploymentForm(deployment, formErrors, isSubmitting, HandleSubmit)
            )
            .Width(Size.Units(140).Max(600));
    }
}

