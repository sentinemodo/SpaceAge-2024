using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using NUnit.Framework;
using NUnit.Framework.Legacy;
using SpaceAge;

namespace UnitTests
{
	[TestFixture]
	public class TNamedObject
	{
		class MockNamedObject : NamedObject
		{
			public MockNamedObject(string name)
				: base(name)
			{
			}
		}

		class MockMultiple : NamedObject, IMultiple
		{
			public MockMultiple(string name)
				: base(name)
			{
			}

			public string FullNameMultiple { get; set; }

			public string ReportNameMultiple
			{
				get { return this.FullNameMultiple + " [" + this.Name + "]"; }
			}
		}

		private MockNamedObject named;

		public TNamedObject()
		{
		}

		[SetUp]
		public void setupDataFile()
		{
			this.named = new MockNamedObject("testName");
		}

		[Test]
		public void SetupTeardown()
		{
            Assert.That(true);
		}

		[Test]
		public void RandomId()
		{
			string name1 = this.named.GenerateRandomIdentifier();
			string name2 = this.named.GenerateRandomIdentifier();
            Assert.That(name1, Is.Not.EqualTo(name2));
		}

		[Test]
		public void LoadXml_NameEnAndDescription_SetsFullNameAndDescription()
		{
			XmlElement el = Element("<entry name=\"iron\" name-en=\"Iron\" description=\"ore\"/>");

			this.named.LoadXml(el);

			Assert.That(this.named.FullName, Is.EqualTo("Iron"));
			Assert.That(this.named.Description, Is.EqualTo("ore"));
		}

		[Test]
		public void LoadXml_MissingNameEn_LeavesExistingFullName()
		{
			this.named.FullName = "Kept";
			XmlElement el = Element("<entry name=\"iron\" description=\"ore\"/>");

			this.named.LoadXml(el);

			Assert.That(this.named.FullName, Is.EqualTo("Kept"));
			Assert.That(this.named.Description, Is.EqualTo("ore"));
		}

		[Test]
		public void LoadMultipleNames_NameEn2Present_SetsFullNameMultiple()
		{
			MockMultiple multiple = new MockMultiple("iron");
			multiple.FullName = "Iron";
			XmlElement el = Element("<entry name=\"iron\" name-en=\"Iron\" name-en2=\"Irons\"/>");

			multiple.LoadMultipleNames(el);

			Assert.That(multiple.FullNameMultiple, Is.EqualTo("Irons"));
		}

		[Test]
		public void LoadMultipleNames_MissingNameEn2_CopiesFullName()
		{
			MockMultiple multiple = new MockMultiple("iron");
			multiple.FullName = "Iron";
			XmlElement el = Element("<entry name=\"iron\" name-en=\"Iron\"/>");

			multiple.LoadMultipleNames(el);

			Assert.That(multiple.FullNameMultiple, Is.EqualTo("Iron"));
		}

		private static XmlElement Element(string xml)
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);
			return doc.DocumentElement;
		}

	}
}
