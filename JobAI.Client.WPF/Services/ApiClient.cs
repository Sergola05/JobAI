using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using JobAI.Shared.Models;
using Newtonsoft.Json;
using GenerateLetterRequestDto = JobAI.Client.WPF.Models.GenerateLetterRequestDto;

namespace JobAI.Client.WPF.Services
{
    public class ApiClient
    {
        private readonly HttpClient _http;

        public ApiClient()
        {
            _http = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5143")
            };
        }

        private async Task<T> ReadAsJsonAsync<T>(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(json);
        }

        private StringContent ToJsonContent(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        public async Task<List<VacancyDto>> GetVacanciesAsync()
        {
            var response = await _http.GetAsync("api/vacancies");
            return await ReadAsJsonAsync<List<VacancyDto>>(response) ?? new List<VacancyDto>();
        }

        public async Task<VacancyDto> CreateVacancyAsync(VacancyDto dto)
        {
            var content = ToJsonContent(dto);
            var response = await _http.PostAsync("api/vacancies", content);
            return await ReadAsJsonAsync<VacancyDto>(response);
        }

        public async Task<CoverLetterDto> GenerateLetterAsync(GenerateLetterRequestDto request)
        {
            var content = ToJsonContent(request);
            var response = await _http.PostAsync("api/CoverLetters/generate", content);
            return await ReadAsJsonAsync<CoverLetterDto>(response);
        }

        public async Task<List<CoverLetterDto>> GetLettersByVacancyAsync(int vacancyId)
        {
            var response = await _http.GetAsync($"api/CoverLetters/by-vacancy/{vacancyId}");
            return await ReadAsJsonAsync<List<CoverLetterDto>>(response) ?? new List<CoverLetterDto>();
        }

        public async Task UpdateLetterAsync(CoverLetterDto dto)
        {
            var content = ToJsonContent(dto);
            var response = await _http.PutAsync($"api/CoverLetters/{dto.Id}", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteVacancyAsync(int vacancyId)
        {
            var response = await _http.DeleteAsync($"api/vacancies/{vacancyId}");
            response.EnsureSuccessStatusCode();
        }
    }
}
