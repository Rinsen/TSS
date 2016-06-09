using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;


namespace TietoCRM.Controllers
{
    public class FeaturesController : Controller
    {
        // GET: Features
        private FeatureService.ClientClient clientReader;
        private FeatureService.ProductClient productReader;
        private FeatureService.FeaturesClient featureReader;
        private ObservableCollection<FeatureService.Client> clientList;
        private ObservableCollection<FeatureService.Product> productList;
        private ObservableCollection<FeatureService.Features> featureList;
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
         //   featureReader = new FeatureService.FeaturesClient();
         
            //featureList = new ObservableCollection<FeatureService.Features>(featureReader.GetClientsFeatures(1,0));
            //featureList = new ObservableCollection<FeatureService.Features>(featureReader.GetClientsFeatures(prodid,clientid));
            //RecursiveTree(featureList);
            //ViewData.Add("Features", htmlString.ToString() );
          //  featureReader.Close();
           

            
            return View();
        }

        public void RecursiveTree(ObservableCollection<FeatureService.Features> currentList)
        {
            htmlString.Append("<ul>");
            foreach (FeatureService.Features item in currentList)
            {
                htmlString.Append("<li><a href='#'>" + item.Text + "<span class='features-id'>" + item.ID + "</span></a>");
                if (item.children != null && item.children.Length > 0)
                {
                    RecursiveTree(new ObservableCollection<FeatureService.Features>(item.children));
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
            featureList = new ObservableCollection<FeatureService.Features>(featureReader.GetClientsFeatures(prodID, clientID));
            //featureList = new ObservableCollection<FeatureService.Features>(featureReader.GetClientsFeatures(prodid,clientid));
            RecursiveTree(featureList);
           
            featureReader.Close();
    
            return htmlString.ToString();
         
        }
    }
}