namespace hotelier_core_app.Model.DTOs.Request
{
    /// <summary>
    /// Data transfer object for sending emails.
    /// </summary>
    public class SendEmailDTO
    {
        /// <summary>
        /// Gets or sets the list of recipient email addresses.
        /// </summary>
        public List<string>? Recipient { get; set; }

        /// <summary>
        /// Gets or sets the subject of the email.
        /// </summary>
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the message body of the email.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the sender.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the template string for the email, if any.
        /// </summary>
        public string? TemplateString { get; set; }

        /// <summary>
        /// Gets or sets the dictionary of attachments for the email.
        /// </summary>
        public Dictionary<string, Stream>? Attachment { get; set; }

        /// <summary>
        /// Gets or sets the list of CC email addresses.
        /// </summary>
        public List<string>? Cc { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendEmailDTO"/> class.
        /// </summary>
        /// <param name="email">The recipient email addresses.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="message">The message body of the email.</param>
        /// <param name="templateString">The template string for the email, if any.</param>
        public SendEmailDTO(List<string> email, string subject, string message, string? templateString = null)
        {
            Recipient = email;
            Subject = subject;
            Message = message;
            Name = string.Empty;
            TemplateString = templateString;
        }
    }
}
