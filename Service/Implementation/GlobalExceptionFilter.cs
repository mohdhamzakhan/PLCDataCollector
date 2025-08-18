﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PLCDataCollector.Service.Implementation
{
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "An unhandled exception occurred");

            var result = new ObjectResult(new
            {
                error = "An error occurred while processing your request",
                timestamp = DateTime.Now,
                path = context.HttpContext.Request.Path
            })
            {
                StatusCode = 500
            };

            context.Result = result;
            context.ExceptionHandled = true;
        }
    }

}
