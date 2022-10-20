<template>
  <v-container>
    <v-card>
      <v-progress-linear v-if="$store.state.leaderboard.loading" class="position-absolute" indeterminate />
      <v-card-title>
        <v-icon>mdi-podium</v-icon> Leaderboard
      </v-card-title>
      <v-card-text>
        <v-table>
          <thead>
            <tr>
              <th>Name</th>
              <th>Average</th>
              <th>Max</th>
              <th>Best</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="stat in $store.state.leaderboard.data" :key="stat.UserId">
              <td>
                <router-link :to="{ name: 'user', params: { id: stat.UserId }}">
                  {{$store.getters.getUsername(stat.UserId)}}
                </router-link>
              </td>
              <td>{{stat.Average}}</td>
              <td>{{stat.Max}}</td>
              <td>{{stat.Min}}</td>
            </tr>
          </tbody>
        </v-table>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script>
export default {
  name: 'Leaderboard',
  created: function() {
    this.$store.dispatch('getLeaderboard');
  }
}
</script>
