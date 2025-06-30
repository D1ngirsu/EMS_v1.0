public class EmployeeInsuranceDto
{
    public int Iid { get; set; }
    public int Eid { get; set; }
    public string InsuranceContent { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal ContributePercent { get; set; }
    public EmployeeDto Employee { get; set; }
}