namespace Microsoft.DataStrategy.Core.Models
{
    public enum SortBy
    {
        Relevance, // enabled and default only when searching by keywords        
        Order,
        Name,
        RecentlyUpdated, // sort by last push date in github
        MostUsedBy, 
        MostReviewed        
    }
}
