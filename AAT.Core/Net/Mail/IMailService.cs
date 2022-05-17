using System.Net.Mail;

namespace AAT.Net.Mail
{
	/// <summary>
	/// Interface used to implement a mail service.
	/// </summary>
	public interface IMailService
	{
		/// <summary>
		/// Sends a mail message.
		/// </summary>
		/// <param name="mailMessage">The mail message to send.</param>
		void Send(MailMessage mailMessage);
	}
}
