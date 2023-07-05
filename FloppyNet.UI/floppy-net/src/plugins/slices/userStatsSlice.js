export default {
  state: {
    userStats: {
      data: [],
      error: null,
      loading: false
    }
  },
  actions: {
    
    async getUserStats(_, { userId }) {
      this.commit('setLoading', { slice: 'userStats', value: true })
      const response = await fetch(`\\stats-${userId}.json`)

      if (response.ok) {
        const data = await response.json()
        this.commit('setUserStats', data)
      }
      else {
        this.commit('setError', { slice: 'userStats', value: 'Failed to fetch user statistics data' })
      }

      this.commit('setLoading', { slice: 'userStats', value: false })
    }
  },
  mutations: {
    setUserStats(state, payload) {
      state.userStats.data = payload
    },
  }
}