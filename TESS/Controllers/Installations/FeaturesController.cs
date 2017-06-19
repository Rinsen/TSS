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
       
        private ObservableCollection<FeatureService.Client> clientList;
        private ObservableCollection<FeatureService.Product> productList;
        private FeatureObservableCollection<FeatureService.Features> featureList;
        private StringBuilder htmlString = new StringBuilder();

        public ActionResult Index()
        {
            clientList = new ObservableCollection<FeatureService.Client>(FeatureServiceProxy.GetClientClient().GetCustomers());
            clientList.RemoveAt(0);
            ViewData.Add("Clients", clientList);

            productList = new ObservableCollection<FeatureService.Product>(FeatureServiceProxy.GetProductClient().GetProducts());
            ViewData.Add("Products",productList);

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
                    htmlString.Append("<li data-feature-id='" + item.Id + "' class='feature-item-expanded' ontouchstart=''><a href='#'>" + item.Text + "<span class='features-id'><table><tr><td>" + item.Id + "</td></tr></table></span></a>");
                }
                else
                {
                    htmlString.Append("<li data-feature-id='" + item.Id + "' ontouchstart=''><a href='#'>" + item.Text + "<span class='features-id'><table><tr><td>" + item.Id + "</td></tr></table></span></a>");
                }
               
                if (item.Children != null && item.Children.Length > 0)
                {
                    RecursiveTree(new ObservableCollection<FeatureService.Features>(item.Children), search);
                }

                htmlString.Append("</li>");
            }
            htmlString.Append("</ul>");

        }

        public String Data()
        {


            int prodID = int.Parse(Request.Form["productID"]);
            int clientID = int.Parse(Request.Form["clientID"]);

            
            featureList = new FeatureObservableCollection<FeatureService.Features>(FeatureServiceProxy.GetFeaturesClient().GetClientsFeatures(prodID, clientID));
            //featureList = new ObservableCollection<FeatureService.Features>(featureReader.GetClientsFeatures(prodid,clientid));
            RecursiveTree(featureList);
 
    
            return htmlString.ToString();
         
        }

        public JsonResult JsonData()
        {
            int featureID = int.Parse(Request.Form["featureID"]);

            var test = FeatureServiceProxy.GetFeaturesClient().GetFeature(featureID);
            return Json(test);
        }

        public String SearchResults()
        {
            

            String filter = Request.Form["filter"];
            int mode = int.Parse(Request.Form["mode"]);
            int prodID = int.Parse(Request.Form["prodID"]);
            int clientID = int.Parse(Request.Form["clientID"]);
            List<int> expandedIDs = (new JavaScriptSerializer()).Deserialize<List<int>>(Request.Form["expandedIDs"]);


            featureList = new FeatureObservableCollection<FeatureService.Features>(FeatureServiceProxy.GetFeaturesClient().GetClientsFeatures(prodID, clientID));
            FeatureObservableCollection<FeatureService.Features> scriptFeatures;
          
            
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
                   
                    defaultVersion = FeatureServiceProxy.GetVersionClient().GetDefaultVersion(prodID);
                    scriptIDs = new ObservableCollection<int>(FeatureServiceProxy.GetFeaturesClient().GetScriptFeatures(filter, prodID, "CSS", defaultVersion.VersionName));
                    scriptFeatures = featureList.scriptFinder(scriptIDs, filter);
                    scriptFeatures.SetExpanded(new ObservableCollection<int>(expandedIDs));
                    RecursiveTree(scriptFeatures,true);
                    

                    break;
                case 4:
                    
                    
                    defaultVersion = FeatureServiceProxy.GetVersionClient().GetDefaultVersion(prodID);
                    scriptIDs = new ObservableCollection<int>(FeatureServiceProxy.GetFeaturesClient().GetScriptFeatures(filter, prodID, "DB", defaultVersion.VersionName));
                    scriptFeatures = featureList.scriptFinder(scriptIDs, filter);
                    scriptFeatures.SetExpanded(new ObservableCollection<int>(expandedIDs));
                    RecursiveTree(scriptFeatures,true);
                    
                    
                    break;
                case 5:
                    
                    
                    defaultVersion = FeatureServiceProxy.GetVersionClient().GetDefaultVersion(prodID);
                    scriptIDs = new ObservableCollection<int>(FeatureServiceProxy.GetFeaturesClient().GetScriptFeatures(filter, prodID, "META", defaultVersion.VersionName));
                    scriptFeatures = featureList.scriptFinder(scriptIDs, filter);
                    scriptFeatures.SetExpanded(new ObservableCollection<int>(expandedIDs));
                    RecursiveTree(scriptFeatures,true);
                    

                    break;
                case 6:
                   
                    
                    defaultVersion = FeatureServiceProxy.GetVersionClient().GetDefaultVersion(prodID);
                    scriptIDs = new ObservableCollection<int>(FeatureServiceProxy.GetFeaturesClient().GetScriptFeatures(filter, prodID, "INI", defaultVersion.VersionName));
                    scriptFeatures = featureList.scriptFinder(scriptIDs, filter);
                    scriptFeatures.SetExpanded(new ObservableCollection<int>(expandedIDs));
                    RecursiveTree(scriptFeatures,true);
                 
                   
                    
                    break;

            }
            return htmlString.ToString();
        }


        public ActionResult Pdf()
        {
            int clientId = int.Parse(Request["client-id"]);
            
            productList = new ObservableCollection<FeatureService.Product>(FeatureServiceProxy.GetProductClient().GetProducts());
            
            foreach (FeatureService.Product item in productList)
            {
                
                pdfProductTree = new FeatureObservableCollection<FeatureService.Features>(FeatureServiceProxy.GetFeaturesClient()
                                        .GetClientsFeatures(item.Id, clientId));
                
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
                feat.Children = null;
                pdfFeatureList.Add(feat);
                TreeToList(new ObservableCollection<FeatureService.Features>(item.Children));
            }
        }
    }
}