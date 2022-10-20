export default {
  state: {
    leaderboard: {
      data: [],
      error: null,
      loading: false
    },
  },
  actions: {
    async getLeaderboard() {
      this.commit('setLoading', { slice: 'leaderboard', value: true })
      const response = await fetch("\\leaderboard.json")

      if (response.ok) {
        const data = await response.json()
        this.commit('setLeaderboard', data.sort((a, b) => a.Average - b.Average))
      }
      else {
        this.commit('setError', { slice: 'leaderboard', value: 'Failed to fetch leaderboard data' })
      }

      this.commit('setLoading', { slice: 'leaderboard', value: false })
    }
  },
  mutations: {
    setLeaderboard(state, payload) {
      state.leaderboard.data = payload
    },
  }
}