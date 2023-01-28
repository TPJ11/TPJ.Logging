namespace TPJ.LoggingTest.Models;

class ListType
{
    public IEnumerable<InnerListItem> SomeListEnumerable { get; set; }
    public List<InnerListItem> SomeList { get; set; }
    public IList<InnerListItem> SomeIList { get; set; }
    public ICollection<InnerListItem> SomeICollection { get; set; }
    public InnerListItem[] SomeArray { get; set; }
}
class InnerListItem
{
    public string Name { get; set; }
    public IEnumerable<InnerListItem> InnerList { get; set; }
}
