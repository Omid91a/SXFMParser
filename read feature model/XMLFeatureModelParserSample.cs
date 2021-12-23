using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace read_feature_model
{
    public class XMLFeatureModelParserSample
    {
        public void parse()
        {

            try
            {

                String featureModelFile = "model.xml";

                /* Creates the Feature Model Object
                 * ********************************
                 * - Constant USE_VARIABLE_NAME_AS_ID indicates that if an ID has not been defined for a feature node
                 *   in the XML file the feature name should be used as the ID. 
                 * - Constant SET_ID_AUTOMATICALLY can be used to let the system create an unique ID for feature nodes 
                 *   without an ID specification
                 *   Note: if an ID is specified for a feature node in the XML file it will always prevail
                 */
                FeatureModel featureModel = new XMLFeatureModel(featureModelFile);

                // Load the XML file and creates the feature model
                featureModel.LoadModel();

                // A feature model object contains a feature tree and a set of contraints			
                // Let's traverse the feature tree first. We start at the root feature in depth first search.
                Debug.WriteLine("FEATURE TREE --------------------------------");
                traverseDFS(featureModel.Root, 0);

                // Now, let's traverse the extra constraints as a CNF formula
                Debug.WriteLine("EXTRA CONSTRAINTS ---------------------------");
                traverseConstraints(featureModel);

                // Now, let's print some statistics about the feature model
                //FeatureModelStatistics stats = new FeatureModelStatistics(featureModel);
                //stats.update();

                //stats.dump();
            }
            catch (Exception e)
            {
                // TODO: handle exception
                Debug.WriteLine(e.Message);
            }
        }

        public void traverseDFS(FeatureTreeNode node, int tab)
        {
            for (int j = 0; j < tab; j++)
            {
                Debug.WriteLine("\t");
            }
            // Root Feature
            if (node is RootNode)
            {
                Debug.WriteLine("Root");
            }
            // Solitaire Feature
            else if (node is SolitaireFeature)
            {
                // Optional Feature
                if (((SolitaireFeature)node).IsOptional)
                    Debug.WriteLine("Optional");
                // Mandatory Feature
                else
                    Debug.WriteLine("Mandatory");
            }
            // Feature Group
            else if (node is FeatureGroup)
            {
                int minCardinality = ((FeatureGroup)node).Min;
                int maxCardinality = ((FeatureGroup)node).Max;
                Debug.WriteLine("Feature Group[" + minCardinality + "," + maxCardinality + "]");
            }
            // Grouped feature
            else
            {
                Debug.WriteLine("Grouped");
            }
            Debug.WriteLine("(ID=" + node.ID + ", NAME=" + node.Name + ")");
            for (int i = 0; i < node.ChildCount(); i++)
            {
                traverseDFS((FeatureTreeNode)node.GetChildAt(i), tab + 1);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="featureModel"></param>
        public void traverseConstraints(FeatureModel featureModel)
        {
            foreach (PropositionalFormula formula in featureModel.GetConstraints())
            {
                Debug.WriteLine(formula);
            }
        }

    }
}
