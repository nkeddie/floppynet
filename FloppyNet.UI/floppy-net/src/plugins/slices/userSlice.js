export default {
  state: {
    users: {
      data: [],
      error: null,
      loading: false
    }
  },
  actions: {
    async getUsers() {
      this.commit('setLoading', { slice: 'users', value: true })
      const response = await fetch("\\users.json")

      if (response.ok) {
        this.commit('setUsers', await response.json())
      }
      else {
        this.commit('setError', { slice: 'users', value: 'Failed to fetch user data' })
      }

      this.commit('setLoading', { slice: 'users', value: false })
    },
  },
  mutations: {
    setUsers (state, payload) {
      state.users.data = payload
    },
  }
}