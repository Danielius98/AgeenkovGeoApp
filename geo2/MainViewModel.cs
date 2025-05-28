using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.Collections.Generic;
using OxyPlot;
using OxyPlot.Series;
using AeroSpectroApp;

public class MainViewModel : INotifyPropertyChanged
{
    private readonly AppDbContext _context = new AppDbContext();

    public ObservableCollection<Client> Clients { get; }
    public ObservableCollection<Project> Projects { get; }
    public ObservableCollection<Area> Areas { get; }
    public ObservableCollection<Profile> Profiles { get; }
    public ObservableCollection<Measurement> Measurements { get; }
    public ObservableCollection<ProjectProfile> ProjectProfiles { get; }

    private Client _selectedClient;
    private Project _selectedProject;
    private Profile _selectedProfile;

    public Client SelectedClient
    {
        get => _selectedClient;
        set
        {
            _selectedClient = value;
            OnPropertyChanged(nameof(SelectedClient));
        }
    }

    public Project SelectedProject
    {
        get => _selectedProject;
        set
        {
            _selectedProject = value;
            OnPropertyChanged(nameof(SelectedProject));
            OnPropertyChanged(nameof(FilteredAreas));
            OnPropertyChanged(nameof(ProfilesForSelectedProject));
        }
    }

    public Profile SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            _selectedProfile = value;
            OnPropertyChanged(nameof(SelectedProfile));
            OnPropertyChanged(nameof(ProjectsForSelectedProfile));
        }
    }

    public ObservableCollection<Area> FilteredAreas =>
        SelectedProject != null
            ? new ObservableCollection<Area>(Areas.Where(a => a.ProjectID == SelectedProject.ProjectID))
            : new ObservableCollection<Area>();

    public ObservableCollection<Project> ProjectsForSelectedProfile =>
        SelectedProfile != null
            ? new ObservableCollection<Project>(ProjectProfiles
                .Where(pp => pp.ProfileID == SelectedProfile.ProfileID)
                .Select(pp => pp.Project))
            : new ObservableCollection<Project>();

    public ObservableCollection<Profile> ProfilesForSelectedProject =>
        SelectedProject != null
            ? new ObservableCollection<Profile>(ProjectProfiles
                .Where(pp => pp.ProjectID == SelectedProject.ProjectID)
                .Select(pp => pp.Profile))
            : new ObservableCollection<Profile>();
    public ICommand ShowSpectrumCommand { get; }
    public ICommand AddClientCommand { get; }
    public ICommand AddProjectCommand { get; }
    public ICommand AddAreaCommand { get; }
    public ICommand AddProfileCommand { get; }
    public ICommand AddMeasurementCommand { get; }
    public ICommand DeleteClientCommand { get; }
    public ICommand DeleteProjectCommand { get; }
    public ICommand DeleteAreaCommand { get; }
    public ICommand DeleteProfileCommand { get; }
    public ICommand DeleteMeasurementCommand { get; }
    public ICommand AddProfileToProjectCommand { get; }
    public ICommand RemoveProfileFromProjectCommand { get; }
    

    public MainViewModel()
    {
        _context.Database.EnsureCreated();

        Clients = new ObservableCollection<Client>(_context.Clients.Include(c => c.Projects).ToList());
        Projects = new ObservableCollection<Project>(_context.Projects
            .Include(p => p.Client)
            .Include(p => p.Areas)
            .Include(p => p.ProjectProfiles)
            .ToList());
        Areas = new ObservableCollection<Area>(_context.Areas
            .Include(a => a.Project)
            .Include(a => a.Profiles)
            .ToList());
        Profiles = new ObservableCollection<Profile>(_context.Profiles
            .Include(p => p.Area)
            .Include(p => p.ProjectProfiles)
            .Include(p => p.Measurements)
            .ToList());
        Measurements = new ObservableCollection<Measurement>(_context.Measurements
            .Include(m => m.Profile)
            .ToList());
        ProjectProfiles = new ObservableCollection<ProjectProfile>(_context.ProjectProfiles
            .Include(pp => pp.Project)
            .Include(pp => pp.Profile)
            .ToList());

        AddClientCommand = new RelayCommand(AddClient);
        AddProjectCommand = new RelayCommand(AddProject);
        AddAreaCommand = new RelayCommand(AddArea);
        AddProfileCommand = new RelayCommand(AddProfile);
        AddMeasurementCommand = new RelayCommand(AddMeasurement);
        DeleteClientCommand = new RelayCommand(DeleteClient, CanDeleteClient);
        DeleteProjectCommand = new RelayCommand(DeleteProject, CanDeleteProject);
        DeleteAreaCommand = new RelayCommand(DeleteArea, CanDeleteArea);
        DeleteProfileCommand = new RelayCommand(DeleteProfile, CanDeleteProfile);
        DeleteMeasurementCommand = new RelayCommand(DeleteMeasurement, CanDeleteMeasurement);
        AddProfileToProjectCommand = new RelayCommand(AddProfileToProject, CanAddProfileToProject);
        RemoveProfileFromProjectCommand = new RelayCommand(RemoveProfileFromProject, CanRemoveProfileFromProject);
        ShowSpectrumCommand = new RelayCommand(ShowSpectrum, CanShowSpectrum);
    }

    private bool CanDeleteClient(object obj) => obj is Client;
    private bool CanDeleteProject(object obj) => obj is Project;
    private bool CanDeleteArea(object obj) => obj is Area;
    private bool CanDeleteProfile(object obj) => obj is Profile;
    private bool CanDeleteMeasurement(object obj) => obj is Measurement;
    private bool CanAddProfileToProject(object obj) => SelectedProfile != null && SelectedProject != null;
    private bool CanRemoveProfileFromProject(object obj) => SelectedProfile != null && SelectedProject != null &&
        ProjectProfiles.Any(pp => pp.ProfileID == SelectedProfile.ProfileID && pp.ProjectID == SelectedProject.ProjectID);
    private bool CanShowSpectrum(object obj) => obj is Measurement;

    private void AddClient(object obj)
    {
        var newClient = new Client { Name = "Новый клиент", ContactInfo = "info@example.com" };
        _context.Clients.Add(newClient);
        _context.SaveChanges();
        Clients.Add(newClient);
        SelectedClient = newClient;
    }

    private void AddProject(object obj)
    {
        if (Clients.Count == 0) return;

        var client = SelectedClient ?? Clients.First();
        var newProject = new Project
        {
            Name = "Новый проект",
            ClientID = client.ClientID,
            Client = client,
            Description = "Описание проекта",
            StartDate = DateTime.Now,
            EndDate = DateTime.Now.AddDays(30)
        };

        _context.Projects.Add(newProject);
        _context.SaveChanges();
        Projects.Add(newProject);
        SelectedProject = newProject;
    }

    private void AddArea(object obj)
    {
        if (SelectedProject == null) return;

        var newArea = new Area
        {
            Name = "Новая зона",
            ProjectID = SelectedProject.ProjectID,
            Project = SelectedProject,
            Coordinates = "0,0"
        };

        _context.Areas.Add(newArea);
        _context.SaveChanges();
        Areas.Add(newArea);
        OnPropertyChanged(nameof(FilteredAreas));
    }

    private void AddProfile(object obj)
    {
        var newProfile = new Profile
        {
            Name = "Новый профиль",
            Type = "Тип профиля",
            StartCoordinates = "0,0",
            EndCoordinates = "0,0"
        };

        if (FilteredAreas.Count > 0)
        {
            newProfile.AreaID = FilteredAreas.First().AreaID;
            newProfile.Area = FilteredAreas.First();
        }

        _context.Profiles.Add(newProfile);
        _context.SaveChanges();
        Profiles.Add(newProfile);
        SelectedProfile = newProfile;
    }

    private void AddMeasurement(object obj)
    {
        if (Profiles.Count == 0) return;

        var profile = SelectedProfile ?? Profiles.First();
        var random = new Random();

        // Генерация тестовых данных спектра
        var spectrumChannels = 100;
        var spectrumData = string.Join(",",
            Enumerable.Range(0, spectrumChannels)
                .Select(i => random.NextDouble() * 1));

        var newMeasurement = new Measurement
        {
            Timestamp = DateTime.Now,
            Latitude = 55.7558,
            Longitude = 37.6173,
            GammaValue = 150.0,
            Altitude = 300.0,
            ProfileID = profile.ProfileID,
            Profile = profile,
            SpectrumData = spectrumData,
            SpectrumChannels = spectrumChannels,
            SpectrumEnergyMin = 0,
            SpectrumEnergyMax = 3000 // 3 кэВ
        };

        _context.Measurements.Add(newMeasurement);
        _context.SaveChanges();
        Measurements.Add(newMeasurement);
    }

    private void ShowSpectrum(object obj)
    {
        if (obj is Measurement measurement)
        {
            var chartWindow = new SpectrumChartWindow(measurement);
            chartWindow.Show();
        }
    }

    private void AddProfileToProject(object obj)
    {
        if (SelectedProfile == null || SelectedProject == null) return;

        var existingLink = _context.ProjectProfiles
            .FirstOrDefault(pp => pp.ProfileID == SelectedProfile.ProfileID && pp.ProjectID == SelectedProject.ProjectID);

        if (existingLink == null)
        {
            var projectProfile = new ProjectProfile
            {
                ProfileID = SelectedProfile.ProfileID,
                ProjectID = SelectedProject.ProjectID,
                Profile = SelectedProfile,
                Project = SelectedProject
            };

            _context.ProjectProfiles.Add(projectProfile);
            _context.SaveChanges();
            ProjectProfiles.Add(projectProfile);

            OnPropertyChanged(nameof(ProjectsForSelectedProfile));
            OnPropertyChanged(nameof(ProfilesForSelectedProject));
        }
    }

    private void RemoveProfileFromProject(object obj)
    {
        if (SelectedProfile == null || SelectedProject == null) return;

        var projectProfile = _context.ProjectProfiles
            .FirstOrDefault(pp => pp.ProfileID == SelectedProfile.ProfileID && pp.ProjectID == SelectedProject.ProjectID);

        if (projectProfile != null)
        {
            _context.ProjectProfiles.Remove(projectProfile);
            _context.SaveChanges();
            ProjectProfiles.Remove(projectProfile);

            OnPropertyChanged(nameof(ProjectsForSelectedProfile));
            OnPropertyChanged(nameof(ProfilesForSelectedProject));
        }
    }

    private void DeleteClient(object obj)
    {
        if (obj is Client client)
        {
            try
            {
                var projectsToRemove = Projects.Where(p => p.ClientID == client.ClientID).ToList();
                foreach (var project in projectsToRemove)
                {
                    _context.Projects.Remove(project);
                    Projects.Remove(project);
                }

                _context.Clients.Remove(client);
                _context.SaveChanges();
                Clients.Remove(client);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}");
            }
        }
    }

    private void DeleteProject(object obj)
    {
        if (obj is Project project)
        {
            try
            {
                var areasToRemove = Areas.Where(a => a.ProjectID == project.ProjectID).ToList();
                foreach (var area in areasToRemove)
                {
                    _context.Areas.Remove(area);
                    Areas.Remove(area);
                }

                var profileLinksToRemove = ProjectProfiles.Where(pp => pp.ProjectID == project.ProjectID).ToList();
                foreach (var link in profileLinksToRemove)
                {
                    _context.ProjectProfiles.Remove(link);
                    ProjectProfiles.Remove(link);
                }

                _context.Projects.Remove(project);
                _context.SaveChanges();
                Projects.Remove(project);

                if (SelectedProject == project)
                {
                    SelectedProject = null;
                }

                OnPropertyChanged(nameof(FilteredAreas));
                OnPropertyChanged(nameof(ProfilesForSelectedProject));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}");
            }
        }
    }



    private void DeleteArea(object obj)
    {
        if (obj is Area area)
        {
            try
            {
                var profilesToRemove = Profiles.Where(p => p.AreaID == area.AreaID).ToList();
                foreach (var profile in profilesToRemove)
                {
                    var profileLinksToRemove = ProjectProfiles.Where(pp => pp.ProfileID == profile.ProfileID).ToList();
                    foreach (var link in profileLinksToRemove)
                    {
                        _context.ProjectProfiles.Remove(link);
                        ProjectProfiles.Remove(link);
                    }

                    var measurementsToRemove = Measurements.Where(m => m.ProfileID == profile.ProfileID).ToList();
                    foreach (var measurement in measurementsToRemove)
                    {
                        _context.Measurements.Remove(measurement);
                        Measurements.Remove(measurement);
                    }

                    _context.Profiles.Remove(profile);
                    Profiles.Remove(profile);
                }

                _context.Areas.Remove(area);
                _context.SaveChanges();
                Areas.Remove(area);

                OnPropertyChanged(nameof(FilteredAreas));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}");
            }
        }
    }

    private void DeleteProfile(object obj)
    {
        if (obj is Profile profile)
        {
            try
            {
                var profileLinksToRemove = ProjectProfiles.Where(pp => pp.ProfileID == profile.ProfileID).ToList();
                foreach (var link in profileLinksToRemove)
                {
                    _context.ProjectProfiles.Remove(link);
                    ProjectProfiles.Remove(link);
                }

                var measurementsToRemove = Measurements.Where(m => m.ProfileID == profile.ProfileID).ToList();
                foreach (var measurement in measurementsToRemove)
                {
                    _context.Measurements.Remove(measurement);
                    Measurements.Remove(measurement);
                }

                _context.Profiles.Remove(profile);
                _context.SaveChanges();
                Profiles.Remove(profile);

                if (SelectedProfile == profile)
                {
                    SelectedProfile = null;
                }

                OnPropertyChanged(nameof(ProjectsForSelectedProfile));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}");
            }
        }
    }

    private void DeleteMeasurement(object obj)
    {
        if (obj is Measurement measurement)
        {
            try
            {
                _context.Measurements.Remove(measurement);
                _context.SaveChanges();
                Measurements.Remove(measurement);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}");
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}