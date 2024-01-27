using CleanArchitecture.Web.ContributorEndpoints;

namespace CleanArchitecture.Web.Endpoints.ContributorEndpoints;

public class ContributorListResponse
{
  public List<ContributorRecord> Contributors { get; set; } = new();
}
