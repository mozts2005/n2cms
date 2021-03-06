#region License

/* Copyright (C) 2007 Cristian Libardo
 *
 * This is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License as
 * published by the Free Software Foundation; either version 2.1 of
 * the License, or (at your option) any later version.
 *
 * This software is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this software; if not, write to the Free
 * Software Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA
 * 02110-1301 USA, or see the FSF site: http://www.fsf.org.
 */

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using N2.Details;
using N2.Installation;
using N2.Integrity;
using N2.Web;
using N2.Web.UI;

namespace N2.Definitions
{
	/// <summary>
	/// Represents the definition of a content item. Expose reflected 
	/// information a types attributes.
	/// </summary>
	public class ItemDefinition : IComparable<ItemDefinition>, ICloneable
	{
		private AttributeExplorer explorer = new AttributeExplorer();
		private string iconUrl;

		#region Constructors
		/// <summary>Creates a new a instance of the ItemDefinition class loading the supplied type.</summary>
		/// <param name="itemType">The item type to define.</param>
		public ItemDefinition(Type itemType, AttributeExplorer explorer)
			: this(itemType)
		{
			this.explorer = explorer;
		}

		/// <summary>Creates a new a instance of the ItemDefinition class loading the supplied type.</summary>
		/// <param name="itemType">The item type to define.</param>
		public ItemDefinition(Type itemType)
		{
			if (!itemType.IsSubclassOf(typeof (ContentItem)))
				throw new ArgumentException("Can only create definitions of content items. This type is not a subclass of N2.ContentItem: " + itemType.FullName, "itemType");

			ItemType = itemType;
			Title = itemType.Name;
			Discriminator = itemType.Name;
			Description = "";
			ToolTip = itemType.FullName;
			SortOrder = 1000;
			AllowedChildFilters = new List<IAllowedDefinitionFilter>();
			AllowedParentFilters = new List<IAllowedDefinitionFilter>();
			ContentTransformers = new List<IContentTransformer>();
			AvailableZones = new List<AvailableZoneAttribute>();
			AllowedZoneNames = new List<string>();
			Editables = new List<IEditable>();
			Containers = new List<IEditableContainer>();
			EditableModifiers = new List<EditorModifierAttribute>();
			Displayables = new List<IDisplayable>();
			NamedOperators = new List<IUniquelyNamed>();
			IsPage = true;
			Enabled = true;
			AllowedIn = AllowedZones.None;
		}

		#endregion

		#region Properties

		/// <summary>Variant of an item with the same discriminator.</summary>
		public string TemplateKey { get; set; }

		/// <summary>Discriminator of an item this item is related to.</summary>
		public string RelatedTo { get; set; }

		/// <summary>Numbers of items of this type in the database.</summary>
		public int NumberOfItems { get; set; }

		/// <summary>Gets or sets how to treat this definition during installation.</summary>
		public InstallerHint Installer { get; set; }

		/// <summary>Whether the defined type is a page or a part.</summary>
		public bool IsPage { get; set; }

		/// <summary>Gets roles or users allowed to edit items defined by this definition.</summary>
		public IList<string> AuthorizedRoles { get; set; }

		/// <summary>Gets the name used when presenting this item class to editors.</summary>
		public string Title { get; set; }

		/// <summary>Gets discriminator value used to to map class when retrieving from persistence. When this is null the type's full name is used.</summary>
		public string Discriminator { get; set; }

		/// <summary>Definitions which are not enabled are not available when creating new items.</summary>
		public bool Enabled { get; set; }

		/// <summary>Gets or sets wheter this definition has been defined. Weirdly enough a definition may exist without beeing defined. To define a definition the class must implement the <see cref="N2.PageDefinitionAttribute"/> or <see cref="PartDefinitionAttribute"/>.</summary>
		public bool IsDefined { get; internal set; }

		/// <summary>Gets the order of this item type when selecting new item in edit mode.</summary>
		public int SortOrder { get; set; }

		/// <summary>Gets the tooltip used when presenting this item class to editors.</summary>
		public string ToolTip { get; set; }

		/// <summary>Gets the description used when presenting this item class to editors.</summary>
		public string Description { get; set; }

		/// <summary>Gets or sets the type of this item.</summary>
		public Type ItemType { get; internal set; }

		/// <summary>Gets zones available in this items of this class.</summary>
		public IList<AvailableZoneAttribute> AvailableZones { get; private set; }

		/// <summary>Gets zones this class of items can be placed in.</summary>
		public IList<string> AllowedZoneNames { get; set; }

		/// <summary>Gets the IconUrl returned by a new instance of the item.</summary>
		public string IconUrl
		{
		    get
		    {
		        return Url.ResolveTokens(iconUrl);
		    }
		    set { iconUrl = value; }
		}

		/// <summary>Gets or sets editables defined for the item.</summary>
		public IList<IEditable> Editables { get; private set; }

		/// <summary>Gets or sets containers defined for the item.</summary>
		public IList<IEditableContainer> Containers { get; private set; }

		/// <summary>Gets or sets all editor modifier attributes for this item.</summary>
		public IList<EditorModifierAttribute> EditableModifiers { get; private set; }

		/// <summary>The container name of editors that have no other container.</summary>
		public string DefaultContainerName { get; set; }

		/// <summary>Gets or sets all editor modifier attributes for this item.</summary>
		[Obsolete("Use EditableModifiers")]
		public IList<EditorModifierAttribute> Modifiers { get { return EditableModifiers; } }

		/// <summary>Default modifiers for the content item before it transitions to a state.</summary>
		public IList<IContentTransformer> ContentTransformers { get; set; }

		/// <summary>Gets or sets displayable attributes defined for the item.</summary>
		public IList<IDisplayable> Displayables { get; private set; }

		/// <summary>Named items associated to a property.</summary>
		public IList<IUniquelyNamed> NamedOperators { get; private set; }

		public AllowedZones AllowedIn { get; set; }

		/// <summary>Filters allowed definitions below this definition.</summary>
		public IList<IAllowedDefinitionFilter> AllowedChildFilters { get; private set; }

		/// <summary>Filters allowed definitions above this definition.</summary>
		public IList<IAllowedDefinitionFilter> AllowedParentFilters { get; private set; }

		#endregion

		#region Methods

		/// <summary>Gets or sets additional child types allowed below this item.</summary>
		public IEnumerable<ItemDefinition> GetAllowedChildren(IDefinitionManager definitions, ContentItem parentItem)
		{
			return definitions.GetDefinitions().AllowedBelow(this, parentItem, definitions);
		}

		public bool IsChildAllowed(IDefinitionManager definitions, ContentItem parentItem, ItemDefinition childDefinition)
		{
			return GetAllowedChildren(definitions, parentItem).Any(d => d.ItemType == childDefinition.ItemType);
		}

		/// <summary>Find out if this item is allowed in a zone.</summary>
		/// <param name="zoneName">The zone name to check.</param>
		/// <returns>True if the item is allowed in the zone.</returns>
		public bool IsAllowedInZone(string zoneName)
		{
			if (AllowedIn == AllowedZones.All)
				return true;
			if (AllowedIn == AllowedZones.AllNamed && !string.IsNullOrEmpty(zoneName))
				return true;

			if (AllowedZoneNames == null)
				return true;

			if (string.IsNullOrEmpty(zoneName) && AllowedZoneNames.Count == 0 && AllowedIn != AllowedZones.AllNamed)
				return true;

			if (AllowedIn == AllowedZones.None)
				return false;

			return AllowedZoneNames.Contains(zoneName);
		}

		/// <summary>Gets the editor modifications for the specified detail name.</summary>
		/// <param name="detailName"></param>
		/// <returns></returns>
		public IList<EditorModifierAttribute> GetModifiers(string detailName)
		{
			var filtered = new List<EditorModifierAttribute>();
			foreach (EditorModifierAttribute a in EditableModifiers)
				if (a.Name == detailName)
					filtered.Add(a);
			return filtered;
		}

		/// <summary>Instantiates a new object of the defined content item class.</summary>
		/// <returns>A new instance of the defined content item type.</returns>
		[Obsolete("Use N2.Factory.Definitions.CreateInstance instead.")]
		public ContentItem CreateInstance(ContentItem parent)
		{
			var item = (ContentItem) Activator.CreateInstance(ItemType);

			item.Parent = parent;
			return item;
		}


		public bool HasZone(string zone)
		{
			if (string.IsNullOrEmpty(zone))
				return true;
			if (AvailableZones != null)
				foreach (AvailableZoneAttribute a in AvailableZones)
					if (a.ZoneName == zone)
						return true;
			return false;
		}

		public bool IsAuthorized(IPrincipal user)
		{
			if (user == null || AuthorizedRoles == null)
				return true;
			foreach (string role in AuthorizedRoles)
				if (string.Equals(user.Identity.Name, role, StringComparison.OrdinalIgnoreCase) || user.IsInRole(role))
					return true;
			return false;
		}

		/// <summary>Adds an allowed zone to the definition's list of allwed zones.</summary>
		/// <param name="zone">The zone name to add.</param>
		public void AddAllowedZone(string zone)
		{
			if (!AllowedZoneNames.Contains(zone))
				AllowedZoneNames.Add(zone);
		}

		/// <summary>Adds an containable editor or container to existing editors and to a container.</summary>
		/// <param name="containable">The editable to add.</param>
		public void Add(IUniquelyNamed containable)
		{
			AddRange(new[] { containable });
		}

		/// <summary>Adds an enumeration of containable editor or container to existing editors and to a container.</summary>
		/// <param name="containables">The editables to add.</param>
		public void AddRange(IEnumerable<IUniquelyNamed> containables)
		{
			foreach (var containable in containables)
			{
				if (containable is IEditable)
					Editables.AddOrReplace(containable as IEditable);
				if (containable is IEditableContainer)
					Containers.AddOrReplace(containable as IEditableContainer);
				if (containable is EditorModifierAttribute)
					EditableModifiers.Add(containable as EditorModifierAttribute);
				if (containable is IDisplayable)
					Displayables.AddOrReplace(containable as IDisplayable);
				if (containable is IContentTransformer)
					ContentTransformers.Add(containable as IContentTransformer);
				
				NamedOperators.Add(containable);
			}
		}

		public IContainable GetContainable(string containableName)
		{
			return Editables.Where(e => e.Name == containableName).OfType<IContainable>().FirstOrDefault()
				?? Containers.Where(c => c.Name == containableName).FirstOrDefault();
		}

		public IEnumerable<IUniquelyNamed> GetNamed(string name)
		{
			return NamedOperators.Where(o => o.Name == name).ToList();
		}

		public void Remove(IUniquelyNamed containable)
		{
			RemoveRange(new[] { containable });
		}

		public void RemoveRange(IEnumerable<IUniquelyNamed> containables)
		{
			foreach (var containable in containables)
			{
				if (containable is IEditable)
					Editables.Remove(containable as IEditable);
				if (containable is IEditableContainer)
					Containers.Remove(containable as IEditableContainer);
				if (containable is IDisplayable)
					Displayables.Remove(containable as IDisplayable);
				if (containable is IContentTransformer)
					ContentTransformers.Remove(containable as IContentTransformer);

				NamedOperators.Remove(containable);
			}
		}

		private HashSet<Type> initializedTypes = new HashSet<Type>();
		public ItemDefinition Initialize(Type type)
		{
			if (initializedTypes.Contains(type))
				return this;

			AddRange(explorer.Find<IUniquelyNamed>(type));
			foreach (ISimpleDefinitionRefiner refiner in type.GetCustomAttributes(typeof(ISimpleDefinitionRefiner), true))
				refiner.Refine(this);

			initializedTypes.Add(type);
			return this;
		}

		#endregion

		#region IComparable<ItemDefinition> Members

		/// <summary>Compares the sort order of this item definition to another.</summary>
		/// <param name="other">The other item definition to compare.</param>
		/// <returns>This items sort order compared to other definition's.</returns>
		public int CompareTo(ItemDefinition other)
		{
			return SortOrder - other.SortOrder;
		}

		#endregion

		#region Equals, GetHashCode & ToString Methods

		public override string ToString()
		{
			return GetDiscriminatorWithTemplateKey();
		}

		public string GetDiscriminatorWithTemplateKey()
		{
			return Discriminator + (TemplateKey != null ? "/" + TemplateKey : null);
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (object.ReferenceEquals(obj, this))
				return true;
			var id = obj as ItemDefinition;
			if (id == null)
				return false;

			return Discriminator == id.Discriminator && TemplateKey == id.TemplateKey;
		}

		#endregion

		#region ICloneable Members

		public ItemDefinition Clone()
		{
			var id = new ItemDefinition(ItemType);
			id.AllowedChildFilters = AllowedChildFilters.ToList();
			id.AllowedIn = AllowedIn;
			id.AllowedParentFilters = AllowedParentFilters.ToList();
			id.AllowedZoneNames = AllowedZoneNames.ToList();
			id.AuthorizedRoles = AuthorizedRoles != null ? AuthorizedRoles.ToList() : AuthorizedRoles;
			id.AvailableZones = AvailableZones.ToList();
			id.Containers = Containers.ToList();
			id.Description = Description;
			id.Discriminator = Discriminator;
			id.Displayables = Displayables.ToList();
			id.Editables = Editables.ToList();
			id.Enabled = Enabled;
			id.IconUrl = IconUrl;
			id.Installer = Installer;
			id.IsDefined = IsDefined;
			id.EditableModifiers = EditableModifiers.ToList();
			id.ContentTransformers = ContentTransformers.ToList();
			id.NumberOfItems = 0;
			id.RelatedTo = RelatedTo;
			id.SortOrder = SortOrder;
			id.TemplateKey = TemplateKey;
			id.Title = Title;
			id.ToolTip = ToolTip;
			return id;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		#endregion

		public bool IsAllowed(string zoneName, IPrincipal user)
		{
			return IsDefined
				&& Enabled
				&& IsAllowedInZone(zoneName)
				&& IsAuthorized(user);
		}
	}

	public static class CollectionExtensions
	{
		public static ICollection<T> AddOrReplace<T>(this ICollection<T> collection, T item) where T : IUniquelyNamed
		{
			return CollectionExtensions.AddOrReplace(collection, item, false);
		}
		public static ICollection<T> AddOrReplace<T>(this ICollection<T> collection, T item, bool replaceIfComparedBefore) where T : IUniquelyNamed
		{
			var existing = collection.FirstOrDefault(i => i.Name == item.Name);
			if (existing != null)
				collection.Remove(existing);

			collection.Add(item);

			return collection;
		}
	}
}