using AAT.Business;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Net.Mail;

namespace AAT.Test.Business
{
	/// <summary>
	/// Tests the CaseFileProcessor class.
	/// </summary>
	[TestClass]
	public class CaseFileProcessorTests
	{

		#region Fields

		private IConfiguration configuration = null;
		private CaseFileProcessor caseFileProcessor = null;

		#endregion

		#region Properties
				
		/// <summary>
		/// Gets the application configuration object.
		/// </summary>
		private IConfiguration Configuration
		{
			get
			{
				if (this.configuration == null)
				{
					this.configuration = new ConfigurationBuilder()
						.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
						.Build();
				}

				return this.configuration;
			}
		}

		/// <summary>
		/// Gets an instance of the case file processor.
		/// </summary>
		private CaseFileProcessor CaseFileProcessor
		{
			get
			{
				if (this.caseFileProcessor == null)
				{
					this.caseFileProcessor = new CaseFileProcessor(
						this.Configuration.GetValue<string>("AAT:CaseFilesFolder"),
						this.Configuration.GetValue<string>("AAT:CaseFileTypes"),
						this.Configuration.GetValue<string>("AAT:PartySchemaFilePath"),
						this.Configuration.GetValue<string>("AAT:AdminEmail"),
						new FakeMailService(),
						new NullLogger<CaseFileProcessor>());
				}

				return this.caseFileProcessor;
			}
		}

		/// <summary>
		/// Gets the ZIP files folder.
		/// </summary>
		private string ZipFilesFolder
		{
			get
			{
				return this.Configuration.GetValue<string>("AAT:ZipFilesFolder");
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Tests the Process method when a ZIP file is valid.
		/// </summary>
    [DataTestMethod()]
		[DataRow("CaseFilesValid.zip")]
		public void ProcessValidTest(string zipFileName)
		{
			// Act
			var success = this.CaseFileProcessor.Process(Path.Combine(this.ZipFilesFolder, zipFileName));

			// Assert
			Assert.IsTrue(success);
		}

		/// <summary>
		/// Tests the Process method when a ZIP file does not exist.
		/// </summary>
    [DataTestMethod()]
		[DataRow("CaseFilesDoesNotExist.zip")]
		public void ProcessFileDoesNotExistTest(string zipFileName)
		{
			// Arrange
			string outputFolder, errorMessage;

			// Act
			var success = this.CaseFileProcessor.Process(Path.Combine(this.ZipFilesFolder, zipFileName), out outputFolder, out errorMessage);

			// Assert
			Assert.IsFalse(success);
			Assert.IsTrue(string.IsNullOrEmpty(outputFolder));
			Assert.IsFalse(string.IsNullOrEmpty(errorMessage));
		}

		/// <summary>
		/// Tests the Process method when a ZIP file is invalid.
		/// </summary>
    [DataTestMethod()]
		[DataRow("CaseFilesInvalid.zip")]
		public void ProcessFileInvalidTest(string zipFileName)
		{
			// Arrange
			string outputFolder, errorMessage;

			// Act
			var success = this.CaseFileProcessor.Process(Path.Combine(this.ZipFilesFolder, zipFileName), out outputFolder, out errorMessage);

			// Assert
			Assert.IsFalse(success);
			Assert.IsTrue(string.IsNullOrEmpty(outputFolder));
			Assert.IsFalse(string.IsNullOrEmpty(errorMessage));
		}

		/// <summary>
		/// Tests the Process method when a ZIP file is valid.
		/// </summary>
    [DataTestMethod()]
		[DataRow("CaseFilesValid.zip")]
		public void ProcessFileValidTest(string zipFileName)
		{
			// Arrange
			string outputFolder, errorMessage;

			// Act
			var success = this.CaseFileProcessor.Process(Path.Combine(this.ZipFilesFolder, zipFileName), out outputFolder, out errorMessage);

			// Assert
			Assert.IsTrue(success);
			Assert.IsFalse(string.IsNullOrEmpty(outputFolder));
			Assert.IsTrue(string.IsNullOrEmpty(errorMessage));
		}

		#endregion

	}
}
