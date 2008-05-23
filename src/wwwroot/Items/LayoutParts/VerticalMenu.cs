using N2.Integrity;
using N2.Templates.Items;
using N2.Definitions;

namespace N2.Templates.UI.Items.LayoutParts
{
	[Disable]
	[Definition("Vertical menu", "VerticalMenu")]
	[RestrictParents(typeof(StartPage))] // The menu is placed on the start page and displayed on all underlying pages
	[AllowedZones(Zones.SiteLeft)]
	public class VerticalMenu : SidebarItem
	{
		[N2.Details.EditableTextBox("Starting depth", 100)]
		public virtual int StartingDepth
		{
			get { return (int)(GetDetail("StartingDepth") ?? 2); }
			set { SetDetail("StartingDepth", value, 1); }
		}

		public override string TemplateUrl
		{
			get { return "~/Layouts/Parts/VerticalMenu.ascx"; }
		}
	}
}