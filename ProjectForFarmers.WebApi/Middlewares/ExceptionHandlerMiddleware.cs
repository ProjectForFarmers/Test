﻿using Serilog;
using System.Net;
using System.Text.Json;
using ApplicationException = ProjectForFarmers.Application.Exceptions.ApplicationException;

namespace ProjectForFarmers.WebApi.Middlewares
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ApplicationException ex)
            {
                Log.Error(ex, "{ErrorMessage}", ex.Message);
                await HandleExceptionAsync(context, ex.UserFacingMessage);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{ErrorMessage}", ex.Message);
                await HandleExceptionAsync(context, ex.Message);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, string message)
        {
            var code = HttpStatusCode.InternalServerError;
            var result = JsonSerializer.Serialize(new { error = message });

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;

            return context.Response.WriteAsync(result);
        }
    }
}
