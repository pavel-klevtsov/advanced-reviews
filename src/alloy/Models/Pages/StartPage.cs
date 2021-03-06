﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Principal;
using AdvancedExternalReviews.DraftContentAreaPreview;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.DataAnnotations;
using EPiServer.SpecializedProperties;
using AlloyTemplates.Models.Blocks;
using EPiServer;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Security;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;

namespace AlloyTemplates.Models.Pages
{
    /// <summary>
    /// Used for the site's start page and also acts as a container for site settings
    /// </summary>
    [ContentType(
        GUID = "19671657-B684-4D95-A61F-8DD4FE60D559",
        GroupName = Global.GroupNames.Specialized)]
    [SiteImageUrl]
    [AvailableContentTypes(
        Availability.Specific,
        Include = new[] { typeof(ContainerPage), typeof(ProductPage), typeof(StandardPage), typeof(ISearchPage), typeof(LandingPage), typeof(ContentFolder) }, // Pages we can create under the start page...
        ExcludeOn = new[] { typeof(ContainerPage), typeof(ProductPage), typeof(StandardPage), typeof(ISearchPage), typeof(LandingPage) })] // ...and underneath those we can't create additional start pages
    public class StartPage : SitePageData
    {
        [Display(
            GroupName = SystemTabNames.Content,
            Order = 320)]
        [CultureSpecific]
        public virtual ContentArea MainContentArea { get; set; }

        [Display(GroupName = Global.GroupNames.SiteSettings, Order = 300)]
        public virtual LinkItemCollection ProductPageLinks { get; set; }

        [Display(GroupName = Global.GroupNames.SiteSettings, Order = 350)]
        public virtual LinkItemCollection CompanyInformationPageLinks { get; set; }

        [Display(GroupName = Global.GroupNames.SiteSettings, Order = 400)]
        public virtual LinkItemCollection NewsPageLinks { get; set; }

        [Display(GroupName = Global.GroupNames.SiteSettings, Order = 450)]
        public virtual LinkItemCollection CustomerZonePageLinks { get; set; }

        [Display(GroupName = Global.GroupNames.SiteSettings)]
        public virtual PageReference GlobalNewsPageLink { get; set; }

        [Display(GroupName = Global.GroupNames.SiteSettings)]
        public virtual PageReference ContactsPageLink { get; set; }

        [Display(GroupName = Global.GroupNames.SiteSettings)]
        public virtual Url FooBar { get; set; }

        [Display(GroupName = Global.GroupNames.SiteSettings)]
        public virtual PageReference SearchPageLink { get; set; }

        [Display(GroupName = Global.GroupNames.SiteSettings)]
        public virtual SiteLogotypeBlock SiteLogotype { get; set; }

        public string ConcatenateFilteredItems()
        {
            if (MainContentArea == null)
            {
                return string.Empty;
            }

            var list = new List<string>();
            foreach (var contentAreaItem in MainContentArea.FilteredItems)
            {
                var content = contentAreaItem.GetContent();
                list.Add(content.Name);
            }

            return string.Join(", ", list);
        }

        public string ConcatenateItems()
        {
            if (MainContentArea == null)
            {
                return string.Empty;
            }

            var list = new List<string>();
            foreach (var contentAreaItem in MainContentArea.Items)
            {
                var content = contentAreaItem.GetContent();
                list.Add(content.Name);
            }

            return string.Join(", ", list);
        }

        public string ConcatenateChildren()
        {
            var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            var items = contentLoader.GetChildren<IContent>(ContentLink);
            var list = new List<string>();
            foreach (var item in items)
            {
                list.Add(item.Name);
            }

            return string.Join(", ", list);
        }

        public string GetChildProducts()
        {
            var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            var items = contentLoader.GetChildren<ProductPage>(ContentLink);
            var list = new List<string>();
            foreach (var item in items)
            {
                list.Add(item.Name);
            }

            return string.Join(", ", list);
        }

        public string GetAlloyPlanProduct()
        {
            var contentLoader = ServiceLocator.Current.GetInstance<IContentLoader>();
            var alloyPlan = contentLoader.Get<ProductPage>(new ContentReference(6));
            return alloyPlan.Name;
        }

        public string ConcatenateChildrenWithReviews()
        {
            var contentLoader = ServiceLocator.Current.GetInstance<ReviewsContentLoader>();
            var items = contentLoader.GetChildrenWithReviews<IContent>(ContentLink);
            var list = new List<string>();
            foreach (var item in items)
            {
                list.Add(item.Name);
            }

            var reference = ContentLink.ToReferenceWithoutVersion();
            ContentProvider provider = ServiceLocator.Current.GetInstance<IContentProviderManager>().ProviderMap.GetProvider(reference);
            string languageID = Language.Name;
            IList<GetChildrenReferenceResult> childrenReferences = provider.GetChildrenReferences<IContent>(reference, languageID, 0, 1000);

            return string.Join(", ", list);
        }
    }

    public class CustomVirtualRole : VirtualRoleProviderBase
    {
        public override bool IsInVirtualRole(IPrincipal principal, object context)
        {
            var pageRouteHelper = ServiceLocator.Current.GetInstance<IPageRouteHelper>();

            // This code is just to make sure that tokens work with custom virtual roles
            // We had a bug from customer that if you use LanguageID from IPageRouteHelper then you would get
            // 404 when generating an EDIT token url.
            // The fix was to set the 'Edit' context later, after the routing is done but before rendering
            var languageId = pageRouteHelper.LanguageID;

            return true;
        }
    }

    [InitializableModule]
    public class CustomContentLoaderInitialization : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            var virtualRoleRepository = ServiceLocator.Current.GetInstance<IVirtualRoleRepository>();
            virtualRoleRepository.Register("VirtualRole", new CustomVirtualRole());
        }

        public void Uninitialize(InitializationEngine context)
        {
        }

        public void Preload(string[] parameters)
        {
        }
    }

}
