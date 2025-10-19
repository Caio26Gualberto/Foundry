import { ThemeProvider } from "@mui/material/styles";
import { CssBaseline } from "@mui/material";
import { SnackbarProvider } from "notistack";
import { AuthProvider } from "./contexts/Auth";
import { AppRouter } from "./components/AppRouter";
import { theme } from "./theme";
import { ConfirmationProvider } from "./contexts/confirmationContext/ConfirmationProvider";
import LanguageBoundary from "./components/common/LanguageBoundary";

function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <ConfirmationProvider>
        <SnackbarProvider
          maxSnack={3}
          anchorOrigin={{
            vertical: "bottom",
            horizontal: "right",
          }}
          autoHideDuration={4000}
        >
          <AuthProvider>
            <LanguageBoundary>
              <AppRouter />
            </LanguageBoundary>
          </AuthProvider>
        </SnackbarProvider>
      </ConfirmationProvider>
    </ThemeProvider>
  );
}

export default App;
