using System;
using System.Collections.Generic;
using System.Text;
using N2.Collections;
using N2.Templates.Items;

namespace N2.Templates
{
	public sealed class Find : N2.Persistence.GenericFind<Items.AbstractRootPage,Items.AbstractStartPage>
	{
		/// <summary>
		/// Gets the item at the specified level.
		/// </summary>
		/// <param name="level">Level = 1 equals start page, level = 2 a child of the start page, and so on.</param>
		/// <returns>An ancestor at the specified level.</returns>
		public static ContentItem AncestorAtLevel(int level)
		{
			return AncestorAtLevel(level, Parents, CurrentPage);
		}

		/// <summary>
		/// Gets the item at the specified level.
		/// </summary>
		/// <param name="level">Level = 1 equals start page, level = 2 a child of the start page, and so on.</param>
		/// <returns>An ancestor at the specified level.</returns>
		public static ContentItem AncestorAtLevel(int level, IEnumerable<ContentItem> parents, ContentItem currentPage)
		{
			ItemList items = new ItemList(parents);
			if (items.Count >= level)
				return items[items.Count - level];
			else if (items.Count == level - 1)
				return currentPage;
			return null;
		}

		public static AbstractStartPage ClosestStartPage
		{
			get
			{
				foreach (ContentItem item in Find.EnumerateParents(CurrentPage, null, true))
				{
					if (item is AbstractStartPage)
					{
						return item as AbstractStartPage;
					}
				}
				return null;
			}
		}
	}
}