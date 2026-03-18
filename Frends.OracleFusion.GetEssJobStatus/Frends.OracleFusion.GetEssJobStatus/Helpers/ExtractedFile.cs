namespace Frends.OracleFusion.GetEssJobStatus.Helpers
{
    /// <summary>
    /// Represents a single file extracted from the ESS job output ZIP archive.
    /// </summary>
    public class ExtractedFile
    {
        /// <summary>
        /// Name of the extracted file.
        /// </summary>
        /// <example>report.csv</example>
        public string FileName { get; set; }

        /// <summary>
        /// Text content of the extracted file.
        /// </summary>
        /// <example>col1,col2\nval1,val2</example>
        public string Content { get; set; }
    }
}
