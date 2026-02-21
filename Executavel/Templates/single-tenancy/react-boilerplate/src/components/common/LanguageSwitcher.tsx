import React from "react";
import { Select, MenuItem, FormControl, InputLabel, Box } from "@mui/material";
import { useTranslation } from "react-i18next";
import i18n from "../../i18n";

const languageNames: Record<string, string> = {
  "en-us": "English",
  "pt-br": "Português",
  "zh-cn": "中文",
  "es-es": "Español",
};

const ccByLng: Record<string, string> = {
  "en-us": "us",
  "pt-br": "br",
  "zh-cn": "cn",
  "es-es": "es",
};

const FlagImg: React.FC<{ lng: string }> = ({ lng }) => {
  const cc = ccByLng[lng];
  if (!cc) return null;
  return (
    <img
      src={`https://flagcdn.com/${cc}.svg`}
      alt=""
      width={18}
      height={12}
      style={{ borderRadius: 2, display: 'block' }}
      referrerPolicy="no-referrer"
    />
  );
};

export const LanguageSwitcher: React.FC = () => {
  const { i18n: i18next } = useTranslation();
  const current = i18next.language?.toLowerCase();
  const supported = (i18n.options?.supportedLngs as string[] | undefined)
    ?.filter((l) => l && l !== "cimode") || ["en-us", "pt-br", "zh-cn", "es-es"]; // fallback

  const handleChange = (e: React.ChangeEvent<{ value: unknown }>) => {
    const lng = String(e.target.value);
    i18n.changeLanguage(lng);
    try {
      localStorage.setItem("i18nextLng", lng);
    } catch { }
  };

  return (
    <FormControl size="small" variant="outlined" sx={{ minWidth: 140 }}>
      <InputLabel id="lang-switcher-label">Language</InputLabel>
      <Select
        labelId="lang-switcher-label"
        label="Language"
        value={current && supported.includes(current) ? current : supported[0]}
        onChange={handleChange as any}
        renderValue={(value) => {
          const v = String(value);
          const name = languageNames[v] || v;
          return (
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', width: '100%', fontFamily: 'Segoe UI Emoji, Arial, sans-serif' }}>
              <span>{name}</span>
              <FlagImg lng={v} />
            </Box>
          );
        }}
      >
        {supported.map((lng) => {
          const name = languageNames[lng] || lng;
          return (
            <MenuItem key={lng} value={lng}>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', width: '100%' }}>
                <span>{name}</span>
                <FlagImg lng={lng} />
              </Box>
            </MenuItem>
          );
        })}
      </Select>
    </FormControl>
  );
};

export default LanguageSwitcher;
