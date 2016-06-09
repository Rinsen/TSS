using Rotativa.MVC;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using TietoCRM.Models;


namespace TietoCRM.Controllers
{
    public class FeaturesController : Controller
    {
        private List<FeatureService.Features> pdfFeatureList = new List<FeatureService.Features>();
        private FeatureObservableCollection<FeatureService.Features> pdfProductTree;

        // GET: Features
        private FeatureService.ClientClient clientReader;
        private FeatureService.ProductClient productReader;
        private FeatureService.FeaturesClient featureReader;
        private ObservableCollection<FeatureService.Client> clientList;
        private ObservableCollection<FeatureService.Product> productList;
        private FeatureObservableCollection<FeatureService.Features> featureList;
        private StringBuilder htmlString = new StringBuilder();
        public ActionResult Index()
        {
            
            clientReader = new FeatureService.ClientClient();
            clientList = new ObservableCollection<FeatureService.Client>(clientReader.GetCustomers());
            clientList.RemoveAt(0);
            ViewData.Add("Clients", clientList);
            clientReader.Close();
            productReader = new FeatureService.ProductClient();
            productList = new ObservableCollection<FeatureService.Product>(productReader.GetProducts());
            ViewData.Add("Products",productList);
            productReader.Close();

            this.ViewData["Title"] = "Feature";
         //   featureReader = new FeatureService.FeaturesClient();
         
            //featureList = new ObservableCollection<FeatureService.Features>(featureReader.GetClientsFeatures(1,0));
            //featureList = new ObservableCollection<FeatureService.Features>(featureReader.GetClientsFeatures(prodid,clientid));
            //RecursiveTree(featureList);
            //ViewData.Add("Features", htmlString.ToString() );
          //  featureReader.Close();
           

            
            return View();
        }

        public void RecursiveTree(ObservableCollection<FeatureService.Features> currentList, bool search = false)
        {
          
            htmlString.Append("<ul>");
            foreach (FeatureService.Features item in currentList)
            {
                if (search && item.IsExpanded)
                {
                    htmlString.Append("<li data-feature-id='" + item.ID + "' class='feature-item-expanded' ontouchstart=''><a href='#'>" + item.Text + "<span class='features-id'><table><tr><td>" + item.ID + "</td><td style='width:120px; text-align:right'>" + item.ArticleNumber + "</td></tr></table></span></a>");
                }
                else
                {
                    htmlString.Append("<li data-feature-id='" + item.ID + "' ontouchstart=''><a href='#'>" + item.Text + "<span class='features-id'><table><tr><td>" + item.ID + "</td><td style='width:120px; text-align:right'>" + item.ArticleNumber + "</td></tr></table></span></a>");
                }
               
                if (item.children != null && item.children.Length > 0)
                {
                    RecursiveTree(new ObservableCollection<FeatureService.Features>(item.children), search);
                }

                htmlString.Append("</li>");
            }
            htmlString.Append("</ul>");

        }

        public String Data()
        {


            int prodID = int.Parse(Request.Form["productID"]);
            int clientID = int.Parse(Request.Form["clientID"]);

            featureReader = new FeatureService.FeaturesClient();
            featureList = new FeatureObservableCollection<FeatureService.Features>(featureReader.GetClientsFeatures(prodID, clientID));
            //featureList = new ObservableCollection<FeatureService.Features>(featureReader.GetClientsFeatures(prodid,clientid));
            RecursiveTree(featureList);
           
            featureReader.Close();
    
            return htmlString.ToString();
         
        }

        public String SearchResults()
        {
            

            String filter = Request.Form["filter"];
            int mode = int.Parse(Request.Form["mode"]);
            int prodID = int.Parse(Request.Form["prodID"]);
            int clientID = int.Parse(Request.Form["clientID"]);
            List<int> expandedIDs = (new JavaScriptSerializer()).Deserialize<List<int>>(Request.Form["expandedIDs"]);

            featureReader = new FeatureService.FeaturesClient();
            featureList = new FeatureObservableCollection<FeatureService.Features>(featureReader.GetClientsFeatures(prodID, clientID));
            FeatureObservableCollection<FeatureService.Features> scriptFeatures;
            featureReader.Close();
            FeatureService.VersionClient versionReader;
            ObservableCollection<int> scriptIDs;
            FeatureService.MyVersion defaultVersion;
            switch (mode)
            {
                case 0:
                    scriptFeatures = featureList.findFeatures(mode, filter);
                    scriptFeatures.SetExpanded(new ObservableCollection<int>(expandedIDs) );
                    RecursiveTree(scriptFeatures, true);
                    break;
                case 1:
                    scriptFeatures = featureList.findFeatures(mode, filter);
                    scriptFeatures.SetExpanded(new ObservableCollection<int>(expandedIDs) );
                    RecursiveTree(scriptFeatures, true);
                    break;
                case 2:
                    scriptFeatures = featureList.findFeatures(mode, filter);
                    scriptFeatures.SetExpanded(new ObservableCollection<int>(expandedIDs));
                    RecursiveTree(scriptFeatures, true);
                    break;
                case 3:
                    versionReader = new FeatureService.VersionClient();
                    featureReader = new FeatureService.FeaturesClient();
                    defaultVersion = versionReader.GetDefaultVersion(prodID);
                    scriptIDs = new ObservableCollection<int>(featureReader.GetScriptFeatures(filter, prodID, "CSS", defaultVersion.versionName));
                    scriptFeatures = featureList.scriptFinder(scriptIDs, filter);
                    scriptFeatures.SetExpanded(new ObservableCollection<int>(expandedIDs));
                    RecursiveTree(scriptFeatures,true);
                    versionReader.Close();
                    featureReader.Close();
                    break;
                case 4:
                    versionReader = new FeatureService.VersionClient();
                    featureReader = new FeatureService.FeaturesClient();
                    defaultVersion = versionReader.GetDefaultVersion(prodID);
                    scriptIDs = new ObservableCollection<int>(featureReader.GetScriptFeatures(filter, prodID, "DB", defaultVersion.versionName));
                    scriptFeatures = featureList.scriptFinder(scriptIDs, filter);
                    scriptFeatures.SetExpanded(new ObservableCollection<int>(expandedIDs));
                    RecursiveTree(scriptFeatures,true);
                    versionReader.Close();
                    featureReader.Close();
                    break;
                case 5:
                    versionReader = new FeatureService.VersionClient();
                    featureReader = new FeatureService.FeaturesClient();
                    defaultVersion = versionReader.GetDefaultVersion(prodID);
                    scriptIDs = new ObservableCollection<int>(featureReader.GetScriptFeatures(filter, prodID, "META", defaultVersion.versionName));
                    scriptFeatures = featureList.scriptFinder(scriptIDs, filter);
                    scriptFeatures.SetExpanded(new ObservableCollection<int>(expandedIDs));
                    RecursiveTree(scriptFeatures,true);
                    versionReader.Close();
                    featureReader.Close();
                    break;
                case 6:
                    versionReader = new FeatureService.VersionClient();
                    featureReader = new FeatureService.FeaturesClient();
                    defaultVersion = versionReader.GetDefaultVersion(prodID);
                    scriptIDs = new ObservableCollection<int>(featureReader.GetScriptFeatures(filter, prodID, "INI", defaultVersion.versionName));
                    scriptFeatures = featureList.scriptFinder(scriptIDs, filter);
                    scriptFeatures.SetExpanded(new ObservableCollection<int>(expandedIDs));
                    RecursiveTree(scriptFeatures,true);
                 
                    versionReader.Close();
                    featureReader.Close();
                    break;

            }
            return htmlString.ToString();
        }


        public ActionResult Pdf()
        {
            int clientId = int.Parse(Request["client-id"]);
            productReader = new FeatureService.ProductClient();
            productList = new ObservableCollection<FeatureService.Product>(productReader.GetProducts());
            
            productReader.Close();


            foreach (FeatureService.Product item in productList)
            {
                featureReader = new FeatureService.FeaturesClient();
                pdfProductTree = new FeatureObservableCollection<FeatureService.Features>(featureReader.GetClientsFeatures(item.ID, clientId));
                featureReader.Close();
                if (pdfProductTree.Count > 0)
                {
                    TreeToList(pdfProductTree);
                    item.IsSelected = true;
                }
                else
                {
                    item.IsSelected = false;
                }
                
            }
            ViewData.Add("Products", productList);
            ViewData.Add("Features", pdfFeatureList);

            ViewAsPdf pdf = new ViewAsPdf("Pdf");
            pdf.RotativaOptions.CustomSwitches = "--print-media-type --header-right \"" + System.DateTime.Now.Date.ToShortDateString() + "\" --header-left \"" + Request["client"] + "\"";
            

            return pdf;
        }
        private void TreeToList(ObservableCollection<FeatureService.Features> pdfFeatureTree)
        {

            foreach (FeatureService.Features item in pdfFeatureTree)
            {
                FeatureService.Features feat = TietoCRM.Models.Cloner<FeatureService.Features>.DeepClone(item);
                feat.children = null;
                pdfFeatureList.Add(feat);
                TreeToList(new ObservableCollection<FeatureService.Features>(item.children));
            }
        }
    }
}