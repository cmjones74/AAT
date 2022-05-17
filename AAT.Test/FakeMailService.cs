using AAT.Net.Mail;
using System.Net.Mail;

namespace AAT.Test
{
	/// <summary>
	/// A fake mail service.
	/// </summary>
	public class FakeMailService : IMailService
	{
		/// <summary>
		/// Sends a mail message.
		/// </summary>
		/// <param name="mailMessage">The mail message to send.</param>
		public void Send(MailMessage mailMessage)
		{
			// Implement code here to send mail message eg. using SmtpClient or Amazon SES
		}
	}
}
