using OnlineRest.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace OnlineRest.Services
{
    public class PdfService
    {
        public byte[] GenerateOrderReceipt(Order order)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .Height(100)
                        .Background(Colors.Blue.Darken3)
                        .Padding(20)
                        .Column(column =>
                        {
                            column.Item().Text("DELICIOUS BITES RESTAURANT")
                                .FontSize(24)
                                .Bold()
                                .FontColor(Colors.White);
                            column.Item().Text("Order Receipt")
                                .FontSize(16)
                                .FontColor(Colors.White);
                        });

                    page.Content()
                        .PaddingVertical(20)
                        .Column(column =>
                        {
                            // Order Information
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text($"Order #: {order.Id}").Bold();
                                    col.Item().Text($"Date: {order.OrderDate:dd MMM yyyy, hh:mm tt}");
                                    col.Item().Text($"Status: {order.Status}").FontColor(GetStatusColor(order.Status));
                                });

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().AlignRight().Text($"Customer: {order.User?.Name}").Bold();
                                    col.Item().AlignRight().Text($"Email: {order.User?.Email}");
                                    col.Item().AlignRight().Text($"Phone: {order.User?.PhoneNumber ?? "N/A"}");
                                });
                            });

                            column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                            // Delivery Address
                            column.Item().PaddingVertical(10).Column(col =>
                            {
                                col.Item().Text("Delivery Address:").Bold();
                                col.Item().Text(order.ShippingAddress);
                            });

                            column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                            // Order Items Table
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });

                                // Header
                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Item").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignCenter().Text("Qty").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Price").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Total").Bold();
                                });

                                // Items
                                if (order.OrderDetails != null)
                                {
                                    foreach (var item in order.OrderDetails)
                                    {
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(item.FoodItem?.Name ?? "Item");
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text(item.Quantity.ToString());
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"${item.UnitPrice:F2}");
                                        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text($"${(item.Quantity * item.UnitPrice):F2}");
                                    }
                                }
                            });

                            column.Item().PaddingVertical(10);

                            // Payment Summary
                            column.Item().AlignRight().Column(col =>
                            {
                                var subtotal = order.TotalAmount;
                                var discount = order.PointsRedeemed * 0.01m; // 1 point = $0.01
                                var finalTotal = subtotal - discount;

                                col.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("Subtotal:");
                                    row.RelativeItem().AlignRight().Text($"${subtotal:F2}");
                                });

                                if (order.PointsRedeemed > 0)
                                {
                                    col.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text($"Points Redeemed ({order.PointsRedeemed} pts):");
                                        row.RelativeItem().AlignRight().Text($"-${discount:F2}").FontColor(Colors.Green.Darken2);
                                    });
                                }

                                col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                                col.Item().PaddingTop(5).Row(row =>
                                {
                                    row.RelativeItem().Text("Total Amount:").Bold().FontSize(14);
                                    row.RelativeItem().AlignRight().Text($"${finalTotal:F2}").Bold().FontSize(14);
                                });

                                col.Item().PaddingTop(10).Row(row =>
                                {
                                    row.RelativeItem().Text("Payment Method:");
                                    row.RelativeItem().AlignRight().Text(order.PaymentMethod);
                                });

                                col.Item().Row(row =>
                                {
                                    row.RelativeItem().Text("Payment Status:");
                                    row.RelativeItem().AlignRight().Text(order.PaymentStatus ?? "Pending")
                                        .FontColor(order.PaymentStatus == "Paid" ? Colors.Green.Darken2 : Colors.Orange.Darken2);
                                });

                                if (order.PointsEarned > 0)
                                {
                                    col.Item().PaddingTop(10).Row(row =>
                                    {
                                        row.RelativeItem().Text("Points Earned:");
                                        row.RelativeItem().AlignRight().Text($"+{order.PointsEarned} pts").FontColor(Colors.Blue.Darken2).Bold();
                                    });
                                }
                            });

                            if (!string.IsNullOrEmpty(order.SpecialInstructions))
                            {
                                column.Item().PaddingTop(20).Column(col =>
                                {
                                    col.Item().Text("Special Instructions:").Bold();
                                    col.Item().Text(order.SpecialInstructions).Italic();
                                });
                            }
                        });

                    page.Footer()
                        .Height(50)
                        .Background(Colors.Grey.Lighten3)
                        .Padding(10)
                        .AlignCenter()
                        .Column(column =>
                        {
                            column.Item().Text("Thank you for your order!").Bold();
                            column.Item().Text("Visit us at www.deliciousbites.com | Call: +92-300-1234567");
                        });
                });
            });

            return document.GeneratePdf();
        }

        private string GetStatusColor(string status)
        {
            return status switch
            {
                "Delivered" => Colors.Green.Darken2.ToString(),
                "Out for Delivery" => Colors.Blue.Darken2.ToString(),
                "Processing" => Colors.Orange.Darken2.ToString(),
                "Cancelled" => Colors.Red.Darken2.ToString(),
                _ => Colors.Grey.Darken2.ToString()
            };
        }
    }
}
