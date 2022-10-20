export default {
  state: {
    groupHistory: {
      data: [],
      error: null,
      loading: false
    }
  },
  actions: {
    async getGroupHistory() {
      this.commit('setLoading', { slice: 'groupHistory', value: true })
      const response = await fetch("\\history-top.json")

      if (response.ok) {
        const data = await response.json()
        this.commit('setGroupHistory', data.sort((a, b) => a.Average - b.Average))
      }
      else {
        this.commit('setError', { slice: 'groupHistory', value: 'Failed to fetch group history data' })
      }

      this.commit('setLoading', { slice: 'groupHistory', value: false })
    },
  },
  mutations: {
    setGroupHistory(state, payload) {
      state.groupHistory.data = payload
    },
  }
}