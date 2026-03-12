namespace Frends.OracleFusion.SubmitEssJob.Helpers;

/// <summary>
/// Request for job submission via ERP Integrations API.
/// </summary>
internal class SubmitJobRequest
{
    /// <summary>
    /// Gets or sets the operation name.
    /// </summary>
    /// <example>submitESSJobRequest</example>
    public string OperationName { get; set; }

    /// <summary>
    /// Gets or sets the job package name.
    /// </summary>
    /// <example>/oracle/apps/ess/financials/payables/invoices/transactions/</example>
    public string JobPackageName { get; set; }

    /// <summary>
    /// Gets or sets the job definition name.
    /// </summary>
    /// <example>APXIAWRE</example>
    public string JobDefName { get; set; }

    /// <summary>
    /// Gets or sets the ESS parameters.
    /// </summary>
    /// <example>12345,#NULL,#NULL,#NULL,678,#NULL,#NULL</example>
    public string ESSParameters { get; set; }
}
