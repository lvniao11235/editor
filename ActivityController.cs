using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Newtonsoft.Json;
using CPFamilyLib.Entity;
using CPFamilyLib;
using CPFamilyLib.ViewModel;
using Aspose.Cells;
using System.IO;

namespace CPFamilyAdminSite.Controllers
{
    public class ActivityController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult Publish()
        {
            //ViewBag.active = CPFamilyCore.GetUniqueActivityGuid();
            //ViewBag.types = CPFamilyCore.GetActivityTypeList();
            ViewBag.active = "fjdslkafjdsalkjfd";
            return View();
        }

        public ActionResult Sign(int id)
        {
            ViewBag.activity = CPFamilyCore.GetActivityById(id);
            ViewBag.register = CPFamilyCore.GetRegistrationList(id);
            return View();
        }

        public FileResult Export(int id)
        {
            ActivityView activity = CPFamilyCore.GetActivityById(id);
            List <RegistrationList> register = CPFamilyCore.GetRegistrationList(id);
            Workbook book = new Workbook();
            Worksheet sheet = book.Worksheets[0];
            Cells cells = sheet.Cells;
            cells[0, 0].Value = "姓名";
            cells[0, 1].Value = "电话";
            for(int i=0; i<register.Count; i++)
            {
                cells[i + 1, 0].Value = register[i].UserName;
                cells[i + 1, 1].Value = register[i].PhoneNum;
            }
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            book.Save(ms, SaveFormat.Xlsx);
            ms.Seek(0, SeekOrigin.Begin);
            return File(ms, "application/vnd.ms-excel", String.Format(@"活动-{0}-报名表.xlsx", activity.Title));
        }

        public ActionResult Modify(int id)
        {
            ActivityView activity = CPFamilyCore.GetActivityById(id);
            ViewBag.images = CPFamilyCore.GetImageList(activity.ActivityGuid);
            ViewBag.types = CPFamilyCore.GetActivityTypeList();
            return View(activity);
        }

        [HttpPost]
        public String Modify(Activity activity, List<int> types)
        {
            CPFamilyCore.UpdateActivity(activity);
            CPFamilyCore.DeleteActivityTypes(activity.Id);
            foreach (int type in types)
            {
                CPFamilyCore.InsertActivityTypes(activity.Id, type);
            }
            return JsonConvert.SerializeObject(new { msg = "success" });
        }

        [HttpPost]
        public String Add(Activity activity, List<int> types)
        {
            CPFamilyCore.AddActivity(activity);
            CPFamilyCore.DeleteActivityTypes(activity.Id);
            foreach(int type in types)
            {
                CPFamilyCore.InsertActivityTypes(activity.Id, type);
            }
            return JsonConvert.SerializeObject(new { msg = "success" });
        }

        [HttpPost]
        public String Delete(int id)
        {
            CPFamilyCore.DeleteActivity(id);
            return JsonConvert.SerializeObject(new { msg = "success" });
        }

        [HttpPost]
        public String Push(int id)
        {
            return JsonConvert.SerializeObject(new { msg = "success" });
        }

        public String Upload(string id)
        {

            if(Request.Files.Count > 0)
            {
                HttpPostedFileBase uploadFile = Request.Files[0] as HttpPostedFileBase;
                if(uploadFile != null && uploadFile.ContentLength > 0)
                {
                    string path = Server.MapPath("/Content/uploads");
                    string name = string.Empty;
                    string guid = string.Empty;
                    while (true)
                    {
                        guid = Guid.NewGuid().ToString().Replace("-", "");
                        name = guid;
                        name += uploadFile.FileName.LastIndexOf(".") != -1 ?
                            uploadFile.FileName.Substring(uploadFile.FileName.LastIndexOf(".")) : "";
                        if(!System.IO.File.Exists(Path.Combine(path, name)))
                        {
                            break;
                        }
                    }
                    uploadFile.SaveAs(Path.Combine(path, name));
                    string url = string.Format("{0}/Content/uploads/{1}", Request.ServerVariables["HTTP_ORIGIN"], name);
                    return JsonConvert.SerializeObject(url); 
                }
            }
            return "error";
        }

        public String DeleteImage(string id)
        {
            CPFamilyCore.DeleteImage(id);
            return JsonConvert.SerializeObject(new { msg = "success" });
        }
        
        public string list(int page, int rows, String query)
        {
           if (!String.IsNullOrEmpty(query))
            {
                return JsonConvert.SerializeObject(new
                {
                    total = CPFamilyCore.GetActivityList().Count,
                    rows = CPFamilyCore.GetActivityList()
                        .Where(x=>x.Title.Contains(query) || x.Content.Contains(query))
                        .Skip((page - 1) * rows).Take(rows)
                });
            }
            else
            {
                return JsonConvert.SerializeObject(new
                {
                    total = CPFamilyCore.GetActivityList().Count,
                    rows = CPFamilyCore.GetActivityList().Skip((page - 1) * rows).Take(rows)
                });
            }
            
        }
    }
}