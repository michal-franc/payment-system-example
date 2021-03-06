using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Moq;
using Xunit;
using FluentAssertions;

using PaymentsSystemExample.Api.Controllers;
using PaymentsSystemExample.Api.Services;
using PaymentsSystemExample.Domain.Adapters.JsonObjects;

namespace PaymentsSystemExample.UnitTests.PaymentControllerTests
{
    public class PaymentControllerTests_WhenCallingPost
    {
        Mock<ILogger<PaymentController>> _loggerMock = new Mock<ILogger<PaymentController>>(); 
        
        [Fact]
        public async Task AndThereIsExeption_ThenReturn500()
        {
            var cultureCode = "en-GB";
            var rawPayment = string.Empty;

            var paymentServiceMock = new Mock<IPaymentService>();
            paymentServiceMock.Setup(x => x.UpdatePayments(It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception());

            var sut = new PaymentController(paymentServiceMock.Object, _loggerMock.Object);

            var result = await sut.Post(rawPayment, cultureCode);

            result.Should().BeOfType<StatusCodeResult>();

            var castedResult = (StatusCodeResult)result;
            castedResult.StatusCode.Should().Be(500);
        }

        [Fact]
        public async Task AndThereIsAValidPayment_Return200_AndUpdatePayment()
        {
            var cultureCode = "en-GB";
            var rawPayment = string.Empty;

            var noValidationErrors = new ValidationErrors();
            var paymentServiceMock = new Mock<IPaymentService>();
            paymentServiceMock.Setup(x => x.UpdatePayments(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(noValidationErrors);

            var sut = new PaymentController(paymentServiceMock.Object, _loggerMock.Object);

            var result = await sut.Post(rawPayment, cultureCode);

            result.Should().BeOfType<OkResult>();
        }

        // We are just mocking this scenario and testing controller behaviour due to validation errors
        [Fact]
        public async Task AndThereIsAInvalidPayment_Return400_AndReturnValidationErrors()
        {
            var cultureCode = "en-GB";
            var rawPayment = string.Empty;

            var validationErrors = new ValidationErrors();
            validationErrors.AddParsingError("amount incorrect value");

            var paymentServiceMock = new Mock<IPaymentService>();
            paymentServiceMock.Setup(x => x.UpdatePayments(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(validationErrors);

            var sut = new PaymentController(paymentServiceMock.Object, _loggerMock.Object);

            IActionResult result = await sut.Post(rawPayment, cultureCode);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AndThereIsNoCultureCode_ThenBadRequest()
        {
            var cultureCode = string.Empty;
            var rawPayment = string.Empty;

            var noValidationErrors = new ValidationErrors();
            var paymentServiceMock = new Mock<IPaymentService>();

            var sut = new PaymentController(paymentServiceMock.Object, _loggerMock.Object);

            var result = await sut.Post(rawPayment, cultureCode);

            result.Should().BeOfType<BadRequestObjectResult>();

            var badRequestObjectResult = (BadRequestObjectResult)result;
            badRequestObjectResult.Value.ToString().Should().Contain("X-CultureCode");
        }
    }
}