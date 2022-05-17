using AAT.Net.Mail;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mail;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace AAT.Business
{
	/// <summary>
	/// Processes case files stored in ZIP files on the AAT file server.
	/// </summary>
	public sealed class CaseFileProcessor
	{

		#region Fields

		private const string PartyXmlFileName = "party.xml";

		#endregion

		#region Constructors

		/// <summary>
		/// Class constructor.
		/// </summary>
		/// <param name="caseFilesFolder">The destination folder for the case files.</param>
		/// <param name="caseFileTypes">The allowable file types for the case folders.</param>
		/// <param name="partySchemaFilePath">The path for schema file used to validate the party XML file.</param>
		/// <param name="adminEmail">The admin email address.</param>
		/// <param name="mailService">The mail service used to send emails to the administrator.</param>
		/// <param name="logger">The logger class to log work done by the system.</param>
		public CaseFileProcessor(string caseFilesFolder, string caseFileTypes, string partySchemaFilePath, string adminEmail, IMailService mailService, ILogger logger)
		{
			this.CaseFilesFolder = caseFilesFolder;
			this.CaseFileTypes = caseFileTypes;
			this.PartySchemaFilePath = partySchemaFilePath;
			this.AdminEmail = adminEmail;
			this.MailService = mailService;
			this.Logger = logger;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or sets the destination folder for the case files.
		/// </summary>
		private string CaseFilesFolder
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the vallowable file types for the case folders.
		/// </summary>
		private string CaseFileTypes
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the path for schema file used to validate the party XML file.
		/// </summary>
		private string PartySchemaFilePath
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the admin email address.
		/// </summary>
		private string AdminEmail
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the mail service used to send emails to the administrator.
		/// </summary>
		private IMailService MailService
		{
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the logger class to log work done by the system.
		/// </summary>
		private ILogger Logger
		{
			get;
			set;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Validates and extracts the case files from a ZIP file and notifies the administrator.
		/// </summary>
		/// <param name="zipFilePath">The file path of the ZIP file to process.</param>
		/// <returns>Whether or not the case files were successfully extracted.</returns>
		public bool Process(string zipFilePath)
		{
			string outputFolder, errorMessage;

			// Log start
			this.Logger.Log(LogLevel.Information, $"Starting processing '{ zipFilePath }'.");

			// Process
			var success = this.Process(zipFilePath, out outputFolder, out errorMessage);
			var message = success
				? $"Case files were successfully extracted to '{ outputFolder }'."
				: $"Case files were unsuccessfully extracted. Reason: '{ errorMessage }'.";

			// Notify administrator
			var mailMessage = new MailMessage();
			mailMessage.From = new MailAddress(this.AdminEmail);
			mailMessage.To.Add(this.AdminEmail);
			mailMessage.Subject = $"Case Files Extract { (success ? "Success" : "Failure") }";
			mailMessage.Body = message;

			this.MailService.Send(mailMessage);

			// Log finish
			this.Logger.Log(LogLevel.Information, message);

			return success;
		}

		/// <summary>
		/// Validates and extracts the case files from a ZIP file.
		/// </summary>
		/// <param name="zipFilePath">The file path of the ZIP file to process.</param>
		/// <param name="outputFolder">The output folder for the extracted case files.</param>
		/// <param name="errorMessage">The error message if an error occurs.</param>
		/// <returns>Whether or not the case files were successfully extracted.</returns>
		public bool Process(string zipFilePath, out string outputFolder, out string errorMessage)
		{
			outputFolder = string.Empty;
			errorMessage = string.Empty;

			try
			{
				// Check ZIP file exists
				if (!File.Exists(zipFilePath))
				{
					errorMessage = $"Unable to find ZIP file '{ zipFilePath }'.";

					return false;
				}

				// Open ZIP file to read contents
				using (var zipFile = ZipFile.OpenRead(zipFilePath))
				{
					// Get the schema file entry
					var entry = zipFile.Entries.SingleOrDefault(e => e.Name.ToLower() == PartyXmlFileName);

					// Check schema file exists
					if (entry == null)
					{
						errorMessage = $"Unable to find '{ PartyXmlFileName }' in ZIP file.";

						return false;
					}

					// Create XML schema object from party XSD file
					var schemaSerializer = new XmlSerializer(typeof(XmlSchema));
					XmlSchema schema;

					using (var stream = File.OpenRead(this.PartySchemaFilePath))
					{
						schema = (XmlSchema)schemaSerializer.Deserialize(stream);
					}

					// Open stream from party XML file, validate party XML file and get application number
					var applicationNo = string.Empty;

					using (var stream = entry.Open())
					{
						// Create XML document object from party XML file and validate against schema (throws exception if validation fails)
						var document = new XmlDocument();
						document.Schemas.Add(schema);
						document.Load(stream);
						document.Validate(null);

						// Get application number
						applicationNo = document.SelectSingleNode("/party/applicationno").InnerText;
					}

					// Check application number
					if (string.IsNullOrEmpty(applicationNo))
					{
						errorMessage = $"Unable to get application number from '{ PartyXmlFileName }'.";

						return false;
					}

					// Create new folder to extract case files to
					var newCaseFilesFolder = $"{ applicationNo }-{ Guid.NewGuid() }";
					Directory.CreateDirectory(Path.Combine(this.CaseFilesFolder, newCaseFilesFolder));

					// Extract case files to directory
					zipFile.Entries.Where(e => this.CaseFileTypes.Split(',').Contains(new FileInfo(e.Name).Extension)).ToList().ForEach(e =>
					{
						e.ExtractToFile(Path.Combine(this.CaseFilesFolder, newCaseFilesFolder, e.Name), true);
					});

					outputFolder = newCaseFilesFolder;
				}
			}
			catch (Exception ex)
			{
				errorMessage = ex.Message;

				return false;
			}

			return true;
		}

		#endregion

	}
}
