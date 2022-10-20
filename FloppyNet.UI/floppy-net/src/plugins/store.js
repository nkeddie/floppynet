import { createStore } from 'vuex';
import userSlice from './slices/userSlice';
import userHistorySlice from './slices/userHistorySlice';
import groupHistorySlice from './slices/groupHistorySlice';
import leaderboardSlice from './slices/leaderboardSlice';

export default createStore({
    state() {
        return {
            ...userSlice.state,
            ...userHistorySlice.state,
            ...groupHistorySlice.state,
            ...leaderboardSlice.state,
        }
    },
    getters: {
      getUsername: (state) => (userId) => state.users.data.find(u => u.UserId === userId).DisplayName
    },
    mutations: {
        ...userSlice.mutations,
        ...userHistorySlice.mutations,
        ...groupHistorySlice.mutations,
        ...leaderboardSlice.mutations,
        setLoading (state, payload) {
          state[payload.slice].loading = payload.value
        },
        setError (state, payload) {
          state[payload.slice].error = payload.value
        }
    },
    actions: {
        ...userSlice.actions,
        ...userHistorySlice.actions,
        ...groupHistorySlice.actions,
        ...leaderboardSlice.actions,
    }
})