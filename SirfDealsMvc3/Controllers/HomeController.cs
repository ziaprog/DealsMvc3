using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SirfDealsMvc3;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using PagedList;


namespace SirfDealsMvc3.Controllers
{ 
    public class HomeController : Controller
    {
        private SirfDealsDBEntities db = new SirfDealsDBEntities();

        //
        // GET: /Home/

        public ViewResult Index(int? page)
        {
           // FlipkartDeals();
            var items = db.Products.OrderByDescending(d => d.Id);
            if (Request.HttpMethod != "GET")
            {
                page = 1;
            }
            int pageSize = 15;
            int pageNumber = (page ?? 1);
            return View(items.ToPagedList(pageNumber,pageSize));
        }
        //FlipKart Deals
        public void FlipkartDeals()
        {

            HtmlWeb hw = new HtmlWeb();
            HtmlAgilityPack.HtmlDocument doc = hw.Load("http://superdeals.homeshop18.com/");

            var deallinks = doc.DocumentNode.SelectNodes("//ul[@id='liveDealsList']//li[.//span[contains(@id,'dealPrice')]]//p[@class='name']//a");

            var dealprice = doc.DocumentNode.SelectNodes("//ul[@id='liveDealsList']//li[.//span[contains(@id,'dealPrice')]]//p[@class='price']//span[contains(@id,'dealPrice')]");

            var couponcode = doc.DocumentNode.SelectNodes("//ul[@id='liveDealsList']//li[.//span[contains(@id,'dealPrice')]]//p[@class='discount']//span[contains(@id,'coupon')]");



            foreach (var link in deallinks)
            {
                try
                {

                    HtmlAttribute linkatt = link.Attributes["href"];
                    //if(!linkatt.Value.Contains("landingid"))
                    Response.Write(linkatt.Value + "<br/>");


                    HtmlWeb hwinner = new HtmlWeb();
                    HtmlAgilityPack.HtmlDocument docinner = hw.Load(linkatt.Value);
                    var productinfo = docinner.DocumentNode.SelectSingleNode(".//body");

                    var itemtitle = productinfo.SelectSingleNode("//span[@itemprop='name']");
                    Response.Write(itemtitle.InnerText + "<br/>");


                    int index = deallinks.IndexOf(link);

                    var itemprice =  dealprice[index].InnerText.Replace("\r", "").Trim();

                    string coupon = couponcode[index].InnerText.Replace("\r", "").Trim();

                    Response.Write(coupon + "<br/>");



                    string couponmsg = "<p style='padding:5px;background-color:#fbfbfb;border:1px solid red'>Use Coupon Code : <strong>" + coupon + "</strong> to get Discount</p>";

                    var itemdesc = productinfo.SelectSingleNode("//div[@class='productDetails']");




                    var itemimage = productinfo.SelectSingleNode("//div[@id='productImageBox']//input[@id='productDefaultImage']");
                    var imgattr = itemimage.Attributes["value"];
                    string imgid = Regex.Replace(itemtitle.InnerText, "[^0-9a-zA-Z]+", "_");

                    //Response.Write("<img src='" + imgattr.Value + "'/>" + "<br/>" + itemtitle.OuterHtml + itemprice);
                    //Response.Write(itemdesc.InnerHtml + couponmsg + "<hr/>");

                    string buttonlink = "<a href='" + linkatt.Value + "' target='_blank'><img class='alignnone size-medium wp-image-11' alt='Buy Now Button' src='http://grabdeal.in/wp-content/uploads/2013/05/Buy-Now-Button-300x103.jpg' width='300' height='103' /></a>";
                    //check if deal already exists
                    //if (CheckIfItemExistsInDB(linkatt.Value))
                    //{
                    //    //do nothing
                    //}
                    //else
                    //{
                    //    //insert into DB

                    //    InsertIntoDB(itemtitle.InnerText, linkatt.Value);

                    //    string button = "<h3>" + itemprice + "</h3>" + "<br/>" + "<br/>" + buttonlink + "<br/>";

                    //    PublishBlogPost(imgattr.Value, itemtitle.InnerText + " " + itemprice, itemdesc.OuterHtml + couponmsg + button, imgid + ".jpg", itemprice, "HomeShop18");

                    //    return;

                    Product p = new Product();
                    p.Title = itemtitle.InnerText;
                    p.AffiliateLink = linkatt.Value;
                    p.Description = itemdesc.OuterHtml;
                    p.ImageUrl = imgattr.Value;
                    p.Price = Convert.ToDecimal( itemprice);

                    if (TryValidateModel(p))
                    {
                        db.Products.Add(p);
                        db.SaveChanges();

                    }

                }

                
                catch (Exception ex)
                {
                    Response.Write(ex.Message);
                }

            }


        }
        //
        // GET: /Home/Details/5

        public ViewResult Details(int id)
        {
            Product product = db.Products.Find(id);
            return View(product);
        }

        //
        // GET: /Home/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Home/Create

        [HttpPost]
        public ActionResult Create(Product product)
        {
            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(product);
        }
        
        //
        // GET: /Home/Edit/5
 
        public ActionResult Edit(int id)
        {
            Product product = db.Products.Find(id);
            return View(product);
        }

        //
        // POST: /Home/Edit/5

        [HttpPost]
        public ActionResult Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(product);
        }

        //
        // GET: /Home/Delete/5
 
        public ActionResult Delete(int id)
        {
            Product product = db.Products.Find(id);
            return View(product);
        }

        //
        // POST: /Home/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {            
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}