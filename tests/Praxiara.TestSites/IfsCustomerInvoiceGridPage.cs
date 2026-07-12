namespace Praxiara.TestSites;

public static class IfsCustomerInvoiceGridPage
{
    public const string Html = """
    <!doctype html>
    <html lang="en">
      <head>
        <meta charset="utf-8">
        <title>IFS Customer Invoices</title>
      </head>
      <body>
        <main>
          <h1>Customer invoices</h1>
          <p id="grid-status" role="status">Showing invoices 1 to 3 of 100</p>
          <div role="grid" aria-label="Customer invoices" aria-rowcount="101" aria-colcount="5">
            <div role="row" aria-label="Invoice headers" aria-rowindex="1">
              <span role="columnheader">Invoice ID</span>
              <span role="columnheader">Customer</span>
              <span role="columnheader">Status</span>
              <span role="columnheader">Amount</span>
              <span role="columnheader">Currency</span>
            </div>
            <div id="invoice-rows">
              <div role="row" aria-label="Invoice 1001, Northwind, open, 1250.00 PLN" aria-rowindex="2">
                <span role="gridcell">1001</span>
                <span role="gridcell">Northwind</span>
                <span role="gridcell">Open</span>
                <span role="gridcell">1250.00</span>
                <span role="gridcell">PLN</span>
              </div>
              <div role="row" aria-label="Invoice 1002, Contoso, paid, 840.50 EUR" aria-rowindex="3">
                <span role="gridcell">1002</span>
                <span role="gridcell">Contoso</span>
                <span role="gridcell">Paid</span>
                <span role="gridcell">840.50</span>
                <span role="gridcell">EUR</span>
              </div>
              <div role="row" aria-label="Invoice 1003, Fabrikam, overdue, 415.75 USD" aria-rowindex="4">
                <span role="gridcell">1003</span>
                <span role="gridcell">Fabrikam</span>
                <span role="gridcell">Overdue</span>
                <span role="gridcell">415.75</span>
                <span role="gridcell">USD</span>
              </div>
            </div>
          </div>
          <button id="next-window" type="button">Load next invoice window</button>
        </main>
        <script>
          document.querySelector('#next-window').addEventListener('click', () => {
            document.querySelector('#invoice-rows').innerHTML = `
              <div role="row" aria-label="Invoice 1004, Adventure Works, open, 99.99 PLN" aria-rowindex="5">
                <span role="gridcell">1004</span>
                <span role="gridcell">Adventure Works</span>
                <span role="gridcell">Open</span>
                <span role="gridcell">99.99</span>
                <span role="gridcell">PLN</span>
              </div>
              <div role="row" aria-label="Invoice 1005, Tailspin Toys, paid, 730.00 EUR" aria-rowindex="6">
                <span role="gridcell">1005</span>
                <span role="gridcell">Tailspin Toys</span>
                <span role="gridcell">Paid</span>
                <span role="gridcell">730.00</span>
                <span role="gridcell">EUR</span>
              </div>
              <div role="row" aria-label="Invoice 1006, Wide World Importers, open, 225.40 USD" aria-rowindex="7">
                <span role="gridcell">1006</span>
                <span role="gridcell">Wide World Importers</span>
                <span role="gridcell">Open</span>
                <span role="gridcell">225.40</span>
                <span role="gridcell">USD</span>
              </div>`;
            document.querySelector('#grid-status').textContent = 'Showing invoices 4 to 6 of 100';
          });
        </script>
      </body>
    </html>
    """;
}