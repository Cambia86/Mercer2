using Mercer.Entities;
using Mercer.Entities.DTO;
using Mercer.Entities.UtilsObj;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mercer.API.Business
{
    public class VAssActivityListBusiness
    {
        public static ActivityDTO getClusterQuestion(int activityId)
        {
            ActivityDTO lstClusterQuestion = new ActivityDTO();
            List<AssCluster> lstCluster = null;
            List<AssQuestion> lstAssQuestion = null;

            using (var context = new MercerDbContext())
            {
                context.Configuration.LazyLoadingEnabled = false;

                //lstCluster = context.AssClusters.Where(x => x.AssActivityId == activityId).ToList();

                lstClusterQuestion.lstClust = (from cluster in context.AssClusters
                                               join clsDesc in context.AssClusterLanguages on cluster.Id equals clsDesc.AssClusterId
                                               where cluster.AssActivityId == activityId
                                               select new ClusterDTO
                                               {
                                                   clusterId = cluster.Id,
                                                   ClusterParentId = cluster.AssClusterParentId,
                                                   clusterTitle = clsDesc.AssClusterTitle,
                                                   clusterDescription = clsDesc.AssClusterDescription,
                                                   clusterTimer = cluster.AssClusterTimer
                                               }).ToList();

                List<Int64> clustId = lstClusterQuestion.lstClust.Select(l => l.clusterId).ToList();

                List<QuestionDTO> lstQuet = (from quest in context.AssQuestions
                                             join questLang in context.AssQuestionLanguages on quest.Id equals questLang.AssQuestionId
                                             where clustId.Contains(quest.AssClusterId)
                                             select new QuestionDTO
                                             {
                                                 clusterId = (int)quest.AssClusterId,
                                                 questionScaleId = quest.AssQuestionScaleId,
                                                 questionTypeId = quest.AssQuestionTypeId,
                                                 questionAdmLanguageId = questLang.AdmLanguageId,
                                                 questionLanguageId = questLang.AdmLanguageId,
                                                 questionTitle = questLang.AssQuestionTitle,
                                                 questionDescription = questLang.AssQuestionDescription
                                             }).ToList();

                foreach (ClusterDTO c in lstClusterQuestion.lstClust)
                {
                    c.lstQuest = lstQuet.Where(x => x.clusterId == c.clusterId).ToList();
                }



                TreeViewDTO cDTO = new TreeViewDTO();

                cDTO = reorderCluster23(cDTO, null, lstClusterQuestion.lstClust);

                //lstClusterQuestion = (from cluster in context.AssClusters
                //                      join clsDesc in context.AssClusterLanguages on cluster.Id equals clsDesc.AssClusterId
                //                      join quest in context.AssQuestions on cluster.Id equals quest.AssClusterId
                //                      join questLang in context.AssQuestionLanguages on quest.Id equals questLang.AssQuestionId
                //                      where cluster.AssActivityId == activityId
                //                      select new ClusterQuestionDTO
                //                      {
                //                          activityId = cluster.AssActivityId,
                //                          clusterTimer = cluster.AssClusterTimer,
                //                          clusterId = cluster.Id,
                //                          clusterTitle = clsDesc.AssClusterTitle,
                //                          clusterDescription = clsDesc.AssClusterDescription,
                //                          questionScaleId = quest.AssQuestionScaleId,
                //                          questionTypeId = quest.AssQuestionTypeId,
                //                          questionAdmLanguageId = questLang.AdmLanguageId,
                //                          questionLanguageId = questLang.AdmLanguageId,
                //                          questionTitle = questLang.AssQuestionTitle,
                //                          questionDescription = questLang.AssQuestionDescription
                //                      }).ToList();
            }

            return lstClusterQuestion;
        }


        private static TreeViewDTO reorderCluster23(TreeViewDTO tree, Int64? parentID, List<ClusterDTO> lstAllCls)
        {

            List<ClusterDTO> lstChild = lstAllCls.Where(x => x.ClusterParentId == parentID).ToList();

            if (lstChild.Count == 0)
            {
                ClusterDTO child = lstAllCls.Where(x => x.clusterId == parentID).FirstOrDefault();
                TreeViewDTO tdto = new TreeViewDTO(child.clusterId, child.clusterTitle, child.lstQuest);
                return tdto;
            }
            else
            {
                tree.Id = parentID==null ? 0 : Convert.ToInt32(parentID);
                if (tree.lstchld.Count == 0)
                    tree.lstchld = new List<TreeViewDTO>();


                for (int i = 0; i < lstChild.Count; i++)
                {
                    tree.lstchld.Add(new TreeViewDTO());
                    TreeViewDTO tdto = reorderCluster23(tree.lstchld[i], lstChild[i].clusterId, lstAllCls);
                    tree.lstchld[i]=tdto;
                    
                }
            }

            return tree;

        }

        public class TreeViewDTO
        {
            public Int64 Id { get; set; }
            public string title { get; set; }
            public List<TreeViewDTO> lstchld { get; set; }
            public List<QuestionDTO> quest { get; set; }

            public TreeViewDTO()
            {
                quest = new List<QuestionDTO>();
                lstchld = new List<TreeViewDTO>();
            }

            public TreeViewDTO(Int64 _clusterId, string _nodeName, List<QuestionDTO> _child)
            {
                Id = _clusterId;
                title = _nodeName;
                quest = _child;
            }
        }
    }
}