using LearningCourse.ViewModels;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace LearningCourse.Services
{
    public class UserService
    {
        private readonly HttpClient _httpClient;

        public UserService()
        {
            _httpClient = App.HttpClient;
        }

        public async Task<UserView> GetCurrentUserInfoAsync(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync(Connection.URL + "user/current");

            if (response.IsSuccessStatusCode)
            {
                var userJson = await response.Content.ReadAsStringAsync();
                var userInfo = JsonConvert.DeserializeObject<UserView>(userJson);
                return userInfo;
            }
            else
            {
                return null;            
            }
        }
    }
}
