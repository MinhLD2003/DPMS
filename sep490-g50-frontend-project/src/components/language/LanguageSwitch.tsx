import { Dropdown, Tooltip } from "antd";
import { useTranslation } from "react-i18next";

const LanguageSwitcher = () => {
    const { i18n } = useTranslation();

    const changeLanguage = (lang: string) => {
        i18n.changeLanguage(lang);
        localStorage.setItem("language", lang);
    };

    const items = [
        {
            key: "vi",
            label: (
                <span>
                    <img src="https://flagcdn.com/w40/vn.png" alt="Vietnam Flag" className="w-5 h-3 inline-block mr-2" />
                    Tiếng Việt
                </span>
            ),
            onClick: () => changeLanguage("vi"),
        },
        {
            key: "en",
            label: (
                <span>
                    <img src="https://flagcdn.com/w40/gb.png" alt="UK Flag" className="w-5 h-3 inline-block mr-2" />
                    English
                </span>
            ),
            onClick: () => changeLanguage("en"),
        },
    ];

    return (
        <Tooltip title="Choose your language">
            <Dropdown menu={{ items }} trigger={["click"]}>
                <span style={{ cursor: "pointer", fontSize: 14, marginLeft: 8 }}>
                    <img
                        src={i18n.language === "vi" ? "https://flagcdn.com/w40/vn.png" : "https://flagcdn.com/w40/gb.png"}
                        alt="Flag"
                        className="w-5 h-3 inline-block mr-2"
                    />
                    {i18n.language === "vi" ? "VIE" : "EN"}
                </span>
            </Dropdown>
        </Tooltip>
    );
};

export default LanguageSwitcher;
