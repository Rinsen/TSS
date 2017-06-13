using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;

namespace TietoCRM.Models
{
    public class FeatureObservableCollection<T> : ObservableCollection<FeatureService.Features>
    {
        public FeatureObservableCollection(FeatureService.Features[] features) : base(features)
        {
            
        }

        public FeatureObservableCollection() : base()
        {

        }
        /// <summary>
        /// This method searches for features with a specific art. nr cr name. uses recursion by using searchChildren to traverse though the tree.
        /// </summary>
        /// <param name="searchType">The script type</param>
        /// <param name="filter">The string for a script to start with</param>
        /// <param name="origFeatures">The list of all features</param>
        /// <returns>collection of features that starts with the correct string</returns>
        public FeatureObservableCollection<FeatureService.Features> findFeatures(int searchType, string filter)
        {
            if (filter.Length == 0)
            {
                return Cloner<FeatureObservableCollection<FeatureService.Features>>.DeepClone(this);
            }
            FeatureObservableCollection<FeatureService.Features> foundFeatures = new FeatureObservableCollection<FeatureService.Features>();
            FeatureObservableCollection<FeatureService.Features> allFeatures = new FeatureObservableCollection<FeatureService.Features>();
            allFeatures = Cloner<FeatureObservableCollection<FeatureService.Features>>.DeepClone(this);
            foreach (FeatureService.Features item in allFeatures)
            {
                this.SearchChildren(searchType, item, filter, allFeatures);

                switch (searchType)
                {
                    case 0:
                        if (item.Children.ToList<FeatureService.Features>().Count != 0)
                        {
                            foundFeatures.Add(item);
                        }
                        else if (item.Text.StartsWith(filter, true, null))
                        {
                            foundFeatures.Add(item);
                        }
                        break;
                    case 1:
                        if (item.Children.ToList<FeatureService.Features>().Count != 0)
                        {
                            foundFeatures.Add(item);
                        }
                        else if (item.Id.ToString().StartsWith(filter, true, null))
                        {
                            foundFeatures.Add(item);
                        }
                        break;
                    case 2:
                        if (item.Children.ToList<FeatureService.Features>().Count != 0)
                        {
                            foundFeatures.Add(item);
                        }
                        else if (item.ArticleNumber.ToString().StartsWith(filter, true, null))
                        {
                            foundFeatures.Add(item);
                        }
                        break;


                }

            }

            return foundFeatures;
        }

        private FeatureObservableCollection<FeatureService.Features> SearchChildren(int searchType, FeatureService.Features root, string filter, FeatureObservableCollection<FeatureService.Features> allFeatures)
        {
            FeatureObservableCollection<FeatureService.Features> foundFeatures = new FeatureObservableCollection<FeatureService.Features>();
            FeatureObservableCollection<FeatureService.Features> Children = new FeatureObservableCollection<FeatureService.Features>();
            Children = Cloner<FeatureObservableCollection<FeatureService.Features>>.DeepClone(root.Children);
            root.Children.ToList<FeatureService.Features>().Clear();
            foreach (FeatureService.Features item in Children)
            {

                this.SearchChildren(searchType, item, filter, allFeatures);
                switch (searchType)
                {
                    case 0:
                        if (item.Children.ToList<FeatureService.Features>().Count != 0)
                        {
                            foundFeatures.Add(item);
                        }
                        else if (item.Text.StartsWith(filter, true, null))
                        {
                            foundFeatures.Add(item);
                        }
                        break;
                    case 1:
                        if (item.Children.ToList<FeatureService.Features>().Count != 0)
                        {
                            foundFeatures.Add(item);
                        }
                        else if (item.Id.ToString().StartsWith(filter, true, null))
                        {
                            foundFeatures.Add(item);
                        }
                        break;
                    case 2:
                        if (item.Children.ToList<FeatureService.Features>().Count != 0)
                        {
                            foundFeatures.Add(item);
                        }
                        else if (item.ArticleNumber.ToString().StartsWith(filter, true, null))
                        {
                            foundFeatures.Add(item);
                        }
                        break;

                }



            }
            root.Children = foundFeatures.ToArray<FeatureService.Features>();
            return foundFeatures;
        }

        /// <summary>
        /// This method filter the scripts. searching recursivly through the tree by using scriptChildrenFinder looking for the IDS.
        /// </summary>
        /// <param name="IDS">The ids of all the features that contains a script that start with the correct string.</param>
        /// <param name="originalTree">The list of all features</param>
        /// <param name="filter">The search string</param>
        /// <returns> A list of features matching the filter.</returns>
        public FeatureObservableCollection<FeatureService.Features> scriptFinder(ObservableCollection<int> IDS, string filter)
        {
            if (filter.Length == 0)
            {
                return Cloner<FeatureObservableCollection<FeatureService.Features>>.DeepClone(this);
            }
            FeatureObservableCollection<FeatureService.Features> foundFeatures = new FeatureObservableCollection<FeatureService.Features>();
            FeatureObservableCollection<FeatureService.Features> allFeatures = new FeatureObservableCollection<FeatureService.Features>();
            allFeatures = Cloner<FeatureObservableCollection<FeatureService.Features>>.DeepClone(this);
            foreach (FeatureService.Features item in allFeatures)
            {
                this.scriptChildrenFinder(IDS, item, allFeatures);
                if (item.Children.ToList<FeatureService.Features>().Count != 0)
                {
                    foundFeatures.Add(item);
                }
                else if (IDS.Contains(item.Id))
                {
                    foundFeatures.Add(item);
                }
            }
            return foundFeatures;
        }


        private FeatureObservableCollection<FeatureService.Features> scriptChildrenFinder(ObservableCollection<int> IDS, FeatureService.Features root, FeatureObservableCollection<FeatureService.Features> tree)
        {
            FeatureObservableCollection<FeatureService.Features> foundFeatures = new FeatureObservableCollection<FeatureService.Features>();
            FeatureObservableCollection<FeatureService.Features> Children = new FeatureObservableCollection<FeatureService.Features>();
            Children = Cloner<FeatureObservableCollection<FeatureService.Features>>.DeepClone(root.Children);
            root.Children.ToList<FeatureService.Features>().Clear();
            foreach (FeatureService.Features item in Children)
            {

                this.scriptChildrenFinder(IDS, item, tree);
                if (item.Children.ToList<FeatureService.Features>().Count != 0)
                {
                    foundFeatures.Add(item);
                }
                else if (IDS.Contains(item.Id))
                {
                    foundFeatures.Add(item);
                }
            }

            root.Children = foundFeatures.ToArray<FeatureService.Features>();
            return foundFeatures;
        }



        /// <summary>
        /// Copy the isexpanded value of each feature to the new tree.
        /// </summary>
        /// <param name="oldList">The list to check values from</param>
        /// <param name="newList">The list to set values to</param>
        public void SetExpanded(ObservableCollection<int> expandedIDs, FeatureObservableCollection<FeatureService.Features> features = null)
        {
            if (features == null)
            {
                features = new FeatureObservableCollection<FeatureService.Features>(this.ToArray<FeatureService.Features>());
            }
            foreach (FeatureService.Features feature in features)
            {
                if (expandedIDs.Contains(feature.Id))
                {
                   

                    feature.IsExpanded = true;
                    if (feature.Children.Length > 0 )
                    {
                        this.SetExpanded(expandedIDs, new FeatureObservableCollection<FeatureService.Features>(feature.Children));
                    }
                }

            }
        }

    }
}