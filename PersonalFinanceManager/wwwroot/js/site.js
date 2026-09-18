(() => {
  const root = document.documentElement;
  if ("serviceWorker" in navigator) navigator.serviceWorker.register("/sw.js");
  const preferredTheme = root.dataset.preferredTheme;
  if (preferredTheme === "dark" || (preferredTheme === "system" && localStorage.getItem("spendwise-theme") === "dark")) root.dataset.theme = "dark";
  document.getElementById("menuButton")?.addEventListener("click", () => document.getElementById("sidebar")?.classList.toggle("open"));
  document.getElementById("themeButton")?.addEventListener("click", () => {
    const dark = root.dataset.theme !== "dark";
    root.dataset.theme = dark ? "dark" : "";
    localStorage.setItem("spendwise-theme", dark ? "dark" : "light");
  });
  const button = document.getElementById("notificationButton");
  const menu = document.getElementById("notificationMenu");
  button?.addEventListener("click", (event) => { event.stopPropagation(); menu?.classList.toggle("show"); });
  document.addEventListener("click", () => menu?.classList.remove("show"));
  const symbol = { PKR: "Rs.", USD: "$", AED: "AED" }[document.body.dataset.currency] || "Rs.";
  if (symbol !== "Rs.") {
    const walker = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT);
    const nodes = []; while (walker.nextNode()) nodes.push(walker.currentNode);
    nodes.forEach((node) => { if (node.parentElement?.tagName !== "SCRIPT") node.nodeValue = node.nodeValue.replaceAll("Rs.", symbol); });
  }
  const format = new Intl.NumberFormat("en-PK", { maximumFractionDigits: 0 });
  const summary = document.getElementById("chartSummary");
  document.querySelectorAll(".bar-pair[data-month]").forEach((bar) => {
    const update = () => {
      document.querySelectorAll(".bar-pair.selected").forEach((item) => item.classList.remove("selected"));
      bar.classList.add("selected");
      if (summary) summary.innerHTML = "<strong>" + bar.dataset.month + "</strong><span>Income <b>Rs. " + format.format(Number(bar.dataset.income)) + "</b></span><span>Expense <b>Rs. " + format.format(Number(bar.dataset.expense)) + "</b></span>";
    };
    bar.addEventListener("mouseenter", update);
    bar.addEventListener("focus", update);
    bar.addEventListener("click", update);
  });
})();
