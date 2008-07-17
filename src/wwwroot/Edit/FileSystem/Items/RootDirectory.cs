﻿using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using N2.Collections;
using System.Collections.Generic;
using N2.Web;
using System.IO;
using N2.Integrity;
using N2.Details;
using N2.Edit.Trash;
using N2.Definitions;

namespace N2.Edit.FileSystem.Items
{
    [Definition("File Folder", SortOrder = 600)]
    [RestrictParents(typeof(IFileSystemContainer))]
    [ItemAuthorizedRoles("Administrators", "admin")]
    public class RootDirectory : AbstractDirectory
    {
        public RootDirectory()
        {
            Visible = false;
            SortOrder = 10000;
        }

        public override string PhysicalPath
        {
            get 
            {
                Url u = Utility.ToAbsolute("~/");
                return GetWebContext().MapPath(u.AppendSegment(Name, ""));; 
            }
            set { throw new InvalidOperationException("Cannot set the root directory's physical path, use Upload property instead.");}
        }

        private IWebContext GetWebContext()
        {
            return N2.Context.Current.Resolve<IWebContext>();
        }
    }
}