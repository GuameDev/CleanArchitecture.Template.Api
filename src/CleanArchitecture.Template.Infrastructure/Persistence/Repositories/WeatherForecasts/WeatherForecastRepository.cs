﻿using CleanArchitecture.Template.Application.WeatherForecasts.Queries.Get.DTOs;
using CleanArchitecture.Template.Application.WeatherForecasts.Queries.GetAll.DTOs;
using CleanArchitecture.Template.Application.WeatherForecasts.Queries.GetById.DTOs;
using CleanArchitecture.Template.Application.WeatherForecasts.Repositories;
using CleanArchitecture.Template.Domain.WeatherForecasts;
using CleanArchitecture.Template.Infrastructure.Persistence.Repositories.Base;
using CleanArchitecture.Template.SharedKernel.Specification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Template.Infrastructure.Persistence.Repositories.WeatherForecasts
{
    internal class WeatherForecastRepository : IWeatherForecastRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WeatherForecastRepository> _logger;

        public WeatherForecastRepository(
            ApplicationDbContext context,
            ILogger<WeatherForecastRepository> logger
            )
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddAsync(WeatherForecast weatherForecast)
        {
            await _context.WeatherForecasts.AddAsync(weatherForecast);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _context.WeatherForecasts.Where(x => x.Id == id).ExecuteDeleteAsync();
        }

        public async Task<GetAllWeatherForecastResponse> GetAllAsync()
        {
            var entities = await _context.WeatherForecasts.ToListAsync();

            return new GetAllWeatherForecastResponse()
            {
                TotalCount = entities.Count,
                Elements = entities.Select(x => new GetAllWeatherForecastListItemResponse()
                {
                    Id = x.Id,
                    Date = x.Date.Value,
                    Summary = x.Summary.ToString(),
                    TemperatureCelsius = x.Temperature.ToCelsius(),
                    TemperatureFahrenheit = x.Temperature.ToFahrenheit()
                }),

            };
        }

        //TODO: Refactor to use criteria Specification Pattern that is already implemented
        public Task<WeatherForecast?> GetByIdAsync(GetWeatherForecastByIdRequest request) => _context.WeatherForecasts
            .FirstOrDefaultAsync(x => x.Id.Equals(request.Id));

        public async Task<GetWeatherForecastListResponse> GetListAsync(ISpecification<WeatherForecast> specification)
        {
            var query = ApplySpecification(specification).AsNoTracking();

            var totalItems = await query.CountAsync();

            var items = await query
                .Select(x => new GetWeatherForecastListItemResponse()
                {
                    Id = x.Id,
                    Date = x.Date.Value,
                    Summary = x.Summary.ToString(),
                    TemperatureCelsius = x.Temperature.ToCelsius(),
                    TemperatureFahrenheit = x.Temperature.ToFahrenheit()
                })
                .ToListAsync();

            return new GetWeatherForecastListResponse
            {
                Elements = items,
                TotalCount = totalItems,
                Page = specification.IsPagingEnabled ? specification.Page : null,
                PageSize = specification.IsPagingEnabled ? specification.PageSize : null,
            };
        }

        private IQueryable<WeatherForecast> ApplySpecification(ISpecification<WeatherForecast> specification) => SpecificationEvaluator<WeatherForecast>
            .GetQuery(_context.WeatherForecasts.AsQueryable(), specification);

        public void Update(WeatherForecast weatherForecast)
        {
            _context.WeatherForecasts.Update(weatherForecast);
        }
    }
}
