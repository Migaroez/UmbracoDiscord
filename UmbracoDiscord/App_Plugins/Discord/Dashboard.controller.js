angular.module("umbraco")
    .controller("Discord.Dashboard", function ($scope, $http) {

        var baseApiUrl = "/umbraco/backoffice/api/discordadmin/";
        var vm = this;
        vm.guilds = [];
        vm.selectedGuild = null;

        vm.roles = [];
        vm.selectedRole = null;

        vm.syncedGroups = [];
        vm.selectedSyncGroup = null;

        vm.groups = [];
        vm.selectedGroup = null;

        vm.addingNewSync = false;

        vm.getGuilds = function () {
            resetSelectedGuild();
            $http.get(baseApiUrl + "Guilds").then((response) => {
                vm.guilds = response.data;
                if (vm.guilds.length === 1) {
                    vm.selectedGuild = vm.guilds[0];
                    vm.getRoles();
                }
            });
        }

        vm.getRoles = function () {
            resetSelectedRole();
            $http.get(baseApiUrl + "Roles?guildId=" + vm.selectedGuild.id).then((response) => {
                vm.roles = response.data;
            });
        }

        vm.getSyncGroups = function () {
            resetSelectedSync();
            $http.get(baseApiUrl + "Syncs?guildId=" + vm.selectedGuild.id + "&roleId=" + vm.selectedRole.id).then((response) => {
                vm.syncedGroups = response.data;
            });
        }

        vm.getGetGroups = function () {
            vm.groups = [];
            vm.selectedGroup = null;
            $http.get(baseApiUrl + "MemberShipGroups").then((response) => {
                vm.groups = response.data;
            });
        }

        vm.startAddNewSync = function () {
            resetSelectedGroup();
            vm.getGetGroups();
            vm.addingNewSync = true;
        }

        vm.selectGuild = function (guild) {
            vm.selectedGuild = guild;
            vm.getRoles();
        }

        vm.selectRole = function (role) {
            vm.selectedRole = role;
            vm.getSyncGroups();
        }

        vm.selectGroup = function (group) {
            vm.selectedGroup = group;
        }

        vm.selectSyncedGroup = function(group) {
            vm.selectedSyncGroup = group;
            console.log(vm.selectedSyncGroup);
        }

        vm.addSync = function() {
            $http.post(baseApiUrl + "RegisterRoleToMemberGroup", { guildId: vm.selectedGuild.id, roleId: vm.selectedRole.id, membershipGroupAlias:vm.selectedGroup}).then((response) => {
                vm.getSyncGroups();
            });
        }

        vm.removeSync = function (syncRemoval) {
            $http.post(baseApiUrl + "RemoveMemberGroupFromRole", { id: vm.selectedSyncGroup.id, syncRemoval: syncRemoval }).then((response) => {
                vm.getSyncGroups();
            });
        }

        vm.cancelAddSync = function () {
            vm.selectedGroup = false;
            vm.addingNewSync = false;
        }


        //private functions
        function resetSelectedGuild() {
            vm.selectGuild = null;

            vm.roles = [];
            resetSelectedRole();
        }

        function resetSelectedRole() {
            vm.selectedRole = null;

            vm.syncedGroups = [];
            resetSelectedSync();
        }

        function resetSelectedSync() {
            vm.selectedSyncGroup = null;
            resetAdding();
        }

        function resetAdding() {
            resetSelectedGroup();
            vm.addingNewSync = false;
        }

        function resetSelectedGroup() {
            vm.selectedGroup = null;
        }

        

        //init
        vm.getGuilds();
    });