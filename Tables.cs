using Dapper.Contrib.Extensions;

namespace SyosetuScraper
{
	[Table("Authors")]
	class SQL_Authors
	{
		[Key]
		public int Author_ID { get; set; }
		public string Author_Code { get; set; }
		public string Author_Page { get; set; }
		public DateTime AddedOn { get; set; }
		public DateTime? LastUpdate { get; set; }
		public DateTime? LastScrape { get; set; }
		public string User_ID { get; set; }
	}

	[Table("Series")]
	class SQL_Series
	{
		[Key]
		public int Series_ID { get; set; }
		public string Series_Code { get; set; }
		public string Series_Page { get; set; }
		public DateTime AddedOn { get; set; }
		public DateTime? LastUpdate { get; set; }
		public DateTime? LastScrape { get; set; }
		public string User_ID { get; set; }
	}

	[Table("Novels")]
	class SQL_Novels
	{
		[Key]
		public int Novel_ID { get; set; }
		public string Novel_Code { get; set; }
		public string Novel_Page { get; set; }
		public string Novel_Type { get; set; }
		public string Novel_Status { get; set; }
		public DateTime? PublicationDate { get; set; }
		public int Chapters { get; set; }
		public int Translated_Chapters { get; set; }
		public int Previous_Chapters { get; set; }
		public DateTime AddedOn { get; set; }
		public DateTime? LastUpdate { get; set; }
		public DateTime LastScrape { get; set; }
		public bool Scrape { get; set; }
		public string User_ID { get; set; }
	}

	[Table("Tags")]
	class SQL_Tags
	{
		[Key]
		public int Tag_ID { get; set; }
		public string Tag_Name { get; set; }
		public string Tag_Meaning { get; set; }
		public DateTime AddedOn { get; set; }
		public int FirstNovelToUse { get; set; }
		public DateTime LastUpdate { get; set; }
		public string User_ID { get; set; }
	}

	[Table("Relationships")]
	class SQL_Relationships
	{
		public int Type { get; set; }
		public int Ranking { get; set; }
		public int Master_ID { get; set; }
		public int Slave_ID { get; set; }
		public DateTime AddedOn { get; set; }
		public string User_ID { get; set; }
	}

	[Table("Names")]
	class SQL_Names
	{
		[Key]
		public int Name_ID { get; set; }
		public string Name_Value { get; set; }
		public DateTime AddedOn { get; set; }
		public string User_ID { get; set; }
	}

	class ViewNovels
	{
		[Key]
		public int Novel_ID { get; set; }
		public string Nickname { get; set; }
		public string English_Name { get; set; }
		public string Japanese_Name { get; set; }
		public string Novel_Type { get; set; }
		public string Novel_Status { get; set; }
		public int Chapters { get; set; }
		public int Previous_Chapters { get; set; }
		public int Translated_Chapters { get; set; }
		public int Percentage { get; set; }
		public DateTime LastScrape { get; set; }
		public bool Scrape { get; set; }
	}
}
