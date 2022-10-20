export default {
  state: {
    userHistory: {
      data: [],
      error: null,
      loading: false
    }
  },
  actions: {
    
    async getUserHistory(_, { userId }) {
      this.commit('setLoading', { slice: 'userHistory', value: true })
      const response = await fetch(`\\history-top-${userId}.json`)

      if (response.ok) {
        const data = await response.json()
        this.commit('setUserHistory', data.sort((a, b) => a.Average - b.Average))
      }
      else {
        this.commit('setError', { slice: 'userHistory', value: 'Failed to fetch user history data' })
      }

      this.commit('setLoading', { slice: 'userHistory', value: false })
    }
  },
  mutations: {
    setUserHistory(state, payload) {
      state.userHistory.data = payload
    },
  }
}